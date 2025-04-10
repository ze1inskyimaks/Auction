using Auction.API.Extensions;
using Auction.API.Options;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtTokenOptions>();

builder.Services
    .AddDataBase(builder.Configuration)
    .AddPasswordOptions()
    .AddDependencyInjection()
    .AddJwtToken(builder.Configuration, jwtSettings!);

var app = builder.Build();

app = ServiceExtension.AddApplicationSettings(app);

await RoleInitializer.EnsureRolesAsync(app.Services);

app.Run();