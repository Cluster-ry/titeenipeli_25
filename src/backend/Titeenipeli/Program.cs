using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using OpenTelemetry;
using OpenTelemetry.Extensions.Propagators;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Titeenipeli.Common.Database;
using Titeenipeli.Common.Database.Services;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Helpers;
using Titeenipeli.InMemoryProvider.MapProvider;
using Titeenipeli.InMemoryProvider.UserProvider;
using Titeenipeli.Middleware;
using Titeenipeli.Options;
using Titeenipeli.Services;
using Titeenipeli.Services.BackgroundServices;

namespace Titeenipeli;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<ApiDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

        var jwtOptions = new JwtOptions();
        builder.Configuration.GetSection("JWT").Bind(jwtOptions);
        builder.Services.AddSingleton(jwtOptions);

        var botOptions = new BotOptions();
        builder.Configuration.GetSection("Bot").Bind(botOptions);
        builder.Services.AddSingleton(botOptions);

        var gameOptions = new GameOptions();
        builder.Configuration.GetSection("Game").Bind(gameOptions);
        builder.Services.AddSingleton(gameOptions);

        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<SpawnGeneratorService>();

        // Adding OpenTelemetry tracing and metrics
        Sdk.SetDefaultTextMapPropagator(new B3Propagator(false));
        builder
            .Services.AddOpenTelemetry()
            .ConfigureResource(ResourceBuilder => ResourceBuilder.AddService(serviceName: "Titeenipeli"))
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .AddSource("Titeenipeli.*")
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService("Titeenipeli"))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
            });

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Cookie,
                Name = "X-Authorization",
                Description = "Bearer Authentication with JWT Token",
                Type = SecuritySchemeType.Http
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string>()
                }
            });
        });

        builder.Services.AddControllers();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults.AuthenticationScheme;

            options.DefaultChallengeScheme =
                JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.ValidIssuer,
                ValidAudience = jwtOptions.ValidAudience,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                TokenDecryptionKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Encryption))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    context.Token = context.Request.Cookies[jwtOptions.CookieName];
                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddControllers()
               .AddNewtonsoftJson(options =>
               {
                   options.SerializerSettings.ContractResolver =
                       new CamelCasePropertyNamesContractResolver();
               });

        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection();
        GrpcServiceRegister.AddSingletonGrpcServices(builder.Services);

        AddBackgroundServices(builder.Services);
        AddRepositoryServices(builder.Services);

        builder.Services.AddSingleton<MapDatabaseWriterService>();
        builder.Services.AddSingleton<UserDatabaseWriterService>();
        builder.Services.AddSingleton<IMapProvider, MapProvider>();
        builder.Services.AddSingleton<IUserProvider, UserProvider>();
        builder.Services.AddSingleton<IMapUpdaterService, MapUpdaterService>();
        builder.Services.AddSingleton<IBackgroundGraphicsService, BackgroundGraphicsService>();
        builder.Services.AddSingleton<IPowerupService, PowerupService>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var dbContext = services.GetRequiredService<ApiDbContext>();
            var mapProvider = services.GetRequiredService<IMapProvider>();
            var userProvider = services.GetRequiredService<IUserProvider>();


            if (app.Environment.IsDevelopment())
            {
                DbFiller.Clear(dbContext);
            }

            bool newDatabase = dbContext.Database.EnsureCreated();
            if (newDatabase)
            {
                DbFiller.Initialize(dbContext, gameOptions, app.Environment.IsDevelopment());
            }

            mapProvider.Initialize(dbContext.Map.Select(pixel => pixel)
                                            .Include(pixel => pixel.User)
                                            .ThenInclude(user => user!.Guild)
                                            .ToList());

            userProvider.Initialize(dbContext.Users.Select(user => user)
                                             .Include(user => user.Guild)
                                             .ToList());
        }

        app.Services.GetRequiredService<ChannelProcessorBackgroundService>().StartAsync(CancellationToken.None);

        app.UseMiddleware<GlobalRoutePrefixMiddleware>("/api/v1");
        app.UsePathBase(new PathString("/api/v1"));

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<JwtDeserializerMiddleware>();

        app.MapControllers();

        GrpcServiceRegister.AddGrpcServices(app);

        app.Run();
    }

    private static void AddBackgroundServices(IServiceCollection services)
    {
        var updateCumulativeScoresServicePeriod = TimeSpan.FromSeconds(15);
        var updatePixelBucketsServicePeriod = TimeSpan.FromMinutes(1);

        services.AddScoped<IUpdateCumulativeScoresService, UpdateCumulativeScoresService>();

        services.AddHostedService(
            serviceProvider =>
                new AsynchronousTimedBackgroundService<IUpdateCumulativeScoresService, UpdateCumulativeScoresService>(
                    serviceProvider,
                    GetNonNullService<ILogger<UpdateCumulativeScoresService>>(
                        serviceProvider),
                    updateCumulativeScoresServicePeriod));

        services.AddScoped<IUpdatePixelBucketsService, UpdatePixelBucketsService>();

        services.AddHostedService(
            serviceProvider =>
                new AsynchronousTimedBackgroundService<
                    IUpdatePixelBucketsService,
                    UpdatePixelBucketsService>(
                    serviceProvider,
                    GetNonNullService<ILogger<UpdatePixelBucketsService>>(
                        serviceProvider),
                    updatePixelBucketsServicePeriod));

        services.AddSingleton<ChannelProcessorBackgroundService>();
        services.AddHostedService<ChannelProcessorBackgroundService>();
    }

    private static void AddRepositoryServices(IServiceCollection services)
    {
        services.AddScoped<IUserRepositoryService, UserRepositoryService>();
        services.AddScoped<IGuildRepositoryService, GuildRepositoryService>();
        services.AddScoped<IMapRepositoryService, MapRepositoryService>();
        services.AddScoped<IGameEventRepositoryService, GameEventRepositoryService>();
        services.AddScoped<ICtfFlagRepositoryService, CtfFlagRepositoryService>();
    }

    private static TService GetNonNullService<TService>(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetService<TService>() ??
               throw new Exception("Unable to discover critical service during startup. " +
                                   $"Service name: {typeof(TService).Name}");
    }
}