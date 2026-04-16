using Auction.Data;
using Auction.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace Auction.API.Extensions;

public static class CategoryInitializer
{
    private static readonly (string Name, string Description)[] DefaultCategories =
    [
        ("Electronics", "Phones, laptops, gadgets, and accessories."),
        ("Home & Garden", "Furniture, decor, and household goods."),
        ("Clothing & Accessories", "Apparel, shoes, bags, and style items."),
        ("Collectibles", "Rare items, cards, coins, and memorabilia."),
        ("Sports & Outdoors", "Fitness gear, sports inventory, and outdoor equipment."),
        ("Books & Education", "Books, textbooks, courses, and educational materials."),
        ("Auto & Moto", "Vehicles, parts, and transport-related accessories."),
        ("Art & Handmade", "Original art, handmade works, and creative crafts.")
    ];

    public static async Task EnsureDefaultCategoriesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var hasAnyCategories = await db.AuctionCategories.AnyAsync();
        if (hasAnyCategories)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var categories = DefaultCategories
            .Select(item => new AuctionCategory
            {
                Name = item.Name,
                Description = item.Description,
                IsActive = true,
                CreatedAt = now
            })
            .ToList();

        await db.AuctionCategories.AddRangeAsync(categories);
        await db.SaveChangesAsync();
    }
}
