using Auction.API.Options;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Auction.BL.Implementation;

public class CloudinaryService
{
    private readonly Cloudinary? _cloudinary;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(
        IOptions<CloudinaryOptions> options,
        IWebHostEnvironment environment,
        ILogger<CloudinaryService> logger)
    {
        _environment = environment;
        _logger = logger;

        var cloudinaryOptions = options.Value;
        var isConfigured =
            !string.IsNullOrWhiteSpace(cloudinaryOptions.CloudName) &&
            !string.IsNullOrWhiteSpace(cloudinaryOptions.ApiKey) &&
            !string.IsNullOrWhiteSpace(cloudinaryOptions.ApiSecret);

        if (!isConfigured)
        {
            return;
        }

        var account = new Account(
            cloudinaryOptions.CloudName,
            cloudinaryOptions.ApiKey,
            cloudinaryOptions.ApiSecret);

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string?> UploadImageAsync(IFormFile file)
    {
        if (file.Length <= 0)
        {
            return null;
        }

        if (_cloudinary is not null)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    PublicId = $"auction_images/{Guid.NewGuid()}"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return uploadResult.SecureUrl.ToString();
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Cloudinary upload failed. Falling back to local file storage.");
            }
        }

        return await SaveLocallyAsync(file);
    }

    private async Task<string?> SaveLocallyAsync(IFormFile file)
    {
        var uploadsDirectory = Path.Combine(_environment.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsDirectory);

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".bin";
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsDirectory, fileName);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(fileStream);

        return $"/uploads/{fileName}";
    }
}
