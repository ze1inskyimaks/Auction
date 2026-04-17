using Auction.API.Extensions;
using Auction.API.Options;
using Auction.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization().AddAuthentication();
builder.Services.AddSignalR();
builder.Services.AddControllers();

builder
    .AddLogging()
    .Services
    .AddDataBase(builder.Configuration)
    .AddPasswordOptions()
    .AddDependencyInjection()
    .Configure<JwtTokenOptions>(builder.Configuration.GetSection("Jwt"))
    .AddJwtToken(builder.Configuration)
    .AddCorsInizializer()
    .AddSwagger()
    .AddRedis(builder.Configuration);

var app = builder.Build();

await ApplyMigrationsWithRetryAsync(app.Services);
await RoleInitializer.EnsureRolesAsync(app.Services);
await CategoryInitializer.EnsureDefaultCategoriesAsync(app.Services);

app = ServiceExtension.AddApplicationSettings(app);

app.Run();

static async Task ApplyMigrationsWithRetryAsync(IServiceProvider services)
{
    const int maxAttempts = 10;
    var delay = TimeSpan.FromSeconds(3);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
            return;
        }
        catch when (attempt < maxAttempts)
        {
            await Task.Delay(delay);
        }
    }

    using var finalScope = services.CreateScope();
    var finalDbContext = finalScope.ServiceProvider.GetRequiredService<AppDbContext>();
    await finalDbContext.Database.MigrateAsync();
}

/*{
    "id": 0,
    "userName": "string",
        "email": "string@gmail.com",
    "password": "stringG1"
}*/

//"Redis": "your-redis-instance.redis.cache.windows.net:6379,password=yourPassword,ssl=True,abortConnect=False"

//http://localhost:5041/swagger/index.html
