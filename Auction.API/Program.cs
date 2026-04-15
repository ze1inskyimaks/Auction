using Auction.API.Extensions;
using Auction.API.Options;

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
    .Configure<CloudinaryOptions>(builder.Configuration.GetSection("Cloudinary"))
    .AddJwtToken(builder.Configuration)
    .AddCorsInizializer()
    .AddSwagger()
    .AddRedis(builder.Configuration);

var app = builder.Build();

await RoleInitializer.EnsureRolesAsync(app.Services);

app = ServiceExtension.AddApplicationSettings(app);

app.Run();

/*{
    "id": 0,
    "userName": "string",
        "email": "string@gmail.com",
    "password": "stringG1"
}*/

//"Redis": "your-redis-instance.redis.cache.windows.net:6379,password=yourPassword,ssl=True,abortConnect=False"

//http://localhost:5041/swagger/index.html
