using Auction.API.Options;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Auction.BL.Implementation;

public class CloudinaryService
{
    private readonly Cloudinary? _cloudinary;

    public CloudinaryService(IOptions<CloudinaryOptions> options)
    {
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
        if (_cloudinary is null)
        {
            return null;
        }

        if (file.Length <= 0)
        {
            return null;
        }

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

        return null;
    }
}
