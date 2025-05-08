using System.Text;
using Auction.API.Options;
using Auction.BL.Implementation;
using Auction.BL.Interface;
using Auction.BL.Services;
using Auction.Data;
using Auction.Data.Implementation;
using Auction.Data.Interface;
using Auction.Data.Model;
using Hangfire;
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
        
        services.AddHangfire(config =>
            config.UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));
        services.AddHangfireServer();
        
        return services;
    }

    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
        services.AddScoped<IAuctionLotService, AuctionLotService>();
        services.AddScoped<IAuctionLotRepository, AuctionLotRepository>();
        services.AddScoped<IAuctionHistoryRepository, AuctionHistoryRepository>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAuctionLobbyService, AuctionLobbyService>();
        services.AddScoped<JwtService>();
        
        services.AddSingleton<IAuctionTimerService, AuctionTimerService>();
        return services;
    }
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }

    public static IServiceCollection AddCorsInizializer(this IServiceCollection service)
    {
        service.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins("http://localhost:3000")  // Вказуємо конкретний домен фронтенду
                    .AllowCredentials()  // Дозволяємо куки
                    .AllowAnyHeader()    // Дозволяємо будь-які заголовки
                    .AllowAnyMethod()    // Дозволяємо будь-які методи
                    .AllowCredentials();
            });
        });
        service.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
        return service;
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

    public static IServiceCollection AddJwtToken(this IServiceCollection services, IConfiguration configuration)
    {
        var tokenOptions = configuration.GetSection("Jwt").Get<JwtTokenOptions>();
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
                        var token = context.Request.Cookies[tokenOptions.JwtSecretWord];
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

    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        return builder;
    }
    
    public static WebApplication AddApplicationSettings(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHangfireDashboard();
        app.MapHub<AuctionHub>("/auctionhub").RequireAuthorization();
        app.MapControllers();

        return app;
    }
}