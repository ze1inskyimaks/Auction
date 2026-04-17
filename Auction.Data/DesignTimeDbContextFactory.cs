using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace Auction.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var candidateApiDirectories = new[]
        {
            Path.Combine(currentDirectory, "Auction.API"),
            Path.Combine(currentDirectory, "..", "Auction.API"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Auction.API")
        }
        .Select(Path.GetFullPath)
        .Distinct()
        .ToArray();

        var apiDirectory = candidateApiDirectories.FirstOrDefault(Directory.Exists);
        if (apiDirectory is null)
        {
            throw new InvalidOperationException(
                "Cannot locate Auction.API directory to read appsettings.json for design-time DbContext.");
        }

        var baseConnection = ReadDefaultConnection(Path.Combine(apiDirectory, "appsettings.json"));
        var developmentConnection = ReadDefaultConnection(Path.Combine(apiDirectory, "appsettings.Development.json"));

        var connectionString = Environment.GetEnvironmentVariable("AUCTION_DB_CONNECTION")
                               ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                               ?? developmentConnection
                               ?? baseConnection;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing in Auction.API appsettings.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string? ReadDefaultConnection(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var json = File.ReadAllText(filePath);
        using var document = JsonDocument.Parse(json);

        if (!document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStringsSection))
        {
            return null;
        }

        if (!connectionStringsSection.TryGetProperty("DefaultConnection", out var defaultConnectionSection))
        {
            return null;
        }

        var value = defaultConnectionSection.GetString();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
