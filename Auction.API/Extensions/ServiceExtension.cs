using System.Text;
using Auction.API.Options;
using Auction.Data;
using Auction.Data.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Auction.API.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddDataBase(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));
        
        return services;
    }

    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
        services.AddSingleton<AppDbContext>();
        return services;
    }

    public static IServiceCollection AddPasswordOptions(this IServiceCollection services)
    {
        // 🔹 Налаштування Identity
        services.AddIdentity<Account, IdentityRole>(options =>
            {
                // Налаштування пароля
                options.Password.RequireDigit = true; // Чи має містити цифри
                options.Password.RequireLowercase = true; // Чи має містити маленькі літери
                options.Password.RequireUppercase = true; // Чи має містити великі літери
                options.Password.RequireNonAlphanumeric = false; // Чи має містити спецсимволи (!@#$%^&)
                options.Password.RequiredLength = 6; // Мінімальна довжина пароля
                options.Password.RequiredUniqueChars = 0; // Мінімальна кількість унікальних символів

                // Інші налаштування
                options.User.RequireUniqueEmail = true; // Чи повинен email бути унікальним
                options.Lockout.MaxFailedAccessAttempts = 5; // Спроби перед блокуванням
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Час блокування
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection AddJwtToken(this IServiceCollection services, IConfiguration configuration, JwtTokenOptions tokenOptions)
    {
        // 🔹 Налаштування автентифікації та JWT
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies[tokenOptions.Word];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = tokenOptions.ValidateIssuer,
                    ValidateAudience = tokenOptions.ValidateAudience,
                    ValidateLifetime = tokenOptions.ValidateLifetime,
                    ValidateIssuerSigningKey = tokenOptions.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(tokenOptions.Key)
                    ),
                };
            });
        return services;
    }

    public static WebApplication AddApplicationSettings(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication(); // 🔥 Має бути перед Authorization!
        app.UseAuthorization();
        app.MapControllers();
        app.UseHttpsRedirection();

        return app;
    }
}