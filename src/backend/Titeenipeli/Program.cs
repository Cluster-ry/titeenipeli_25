using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Titeenipeli.Common.Database;
using Titeenipeli.Common.Database.Services;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Helpers;
using Titeenipeli.Middleware;
using Titeenipeli.Options;
using Titeenipeli.Services;
using Titeenipeli.Services.BackgroundServices;

namespace Titeenipeli;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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

        builder.Services.AddScoped<JwtService>();
        builder.Services.AddScoped<SpawnGeneratorService>();

        // Adding OpenTelemetry tracing and metrics
        IOpenTelemetryBuilder openTelemetry = builder.Services.AddOpenTelemetry();

        string openTelemetryEndpoint = builder.Configuration["OpenTelemetryEndpoint"] ?? "localhost:4318";

        openTelemetry.ConfigureResource(resource =>
            resource.AddService(builder.Environment.ApplicationName));

        openTelemetry.WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel");

            metrics.AddOtlpExporter((exporterOptions, metricReaderOptions) =>
            {
                exporterOptions.Endpoint = new Uri(openTelemetryEndpoint);
                exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
            });
        });

        openTelemetry.WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation();

            tracing.AddOtlpExporter(exporterOptions =>
            {
                exporterOptions.Endpoint = new Uri(openTelemetryEndpoint);
                exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
        });

        // Adding OpenTelemetry logging
        builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter(exporterOptions =>
        {
            exporterOptions.Endpoint = new Uri(openTelemetryEndpoint);
            exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
        }));

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

        builder.Services
               .AddSingleton<IMapUpdaterService, MapUpdaterService>();
        builder.Services
               .AddSingleton<IBackgroundGraphicsService, BackgroundGraphicsService>();

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

        WebApplication app = builder.Build();

        using (IServiceScope scope = app.Services.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;
            var dbContext = services.GetRequiredService<ApiDbContext>();

            DbFiller.Clear(dbContext);
            dbContext.Database.EnsureCreated();
            DbFiller.Initialize(dbContext, gameOptions);
        }

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
        TimeSpan updateCumulativeScoresServicePeriod = TimeSpan.FromMinutes(1);
        TimeSpan updatePixelBucketsServicePeriod = TimeSpan.FromMinutes(1);

        services
            .AddScoped<IUpdateCumulativeScoresService,
                UpdateCumulativeScoresService>();
        services.AddHostedService(
            serviceProvider =>
                new AsynchronousTimedBackgroundService<
                    IUpdateCumulativeScoresService,
                    UpdateCumulativeScoresService>(
                    serviceProvider,
                    GetNonNullService<ILogger<UpdateCumulativeScoresService>>(
                        serviceProvider),
                    updateCumulativeScoresServicePeriod));

        services
            .AddScoped<IUpdatePixelBucketsService,
                UpdatePixelBucketsService>();
        services.AddHostedService(
            serviceProvider =>
                new AsynchronousTimedBackgroundService<
                    IUpdatePixelBucketsService,
                    UpdatePixelBucketsService>(
                    serviceProvider,
                    GetNonNullService<ILogger<UpdatePixelBucketsService>>(
                        serviceProvider),
                    updatePixelBucketsServicePeriod));
    }

    private static void AddRepositoryServices(IServiceCollection services)
    {
        services
            .AddScoped<IUserRepositoryService,
                UserRepositoryService>();

        services
            .AddScoped<IGuildRepositoryService,
                GuildRepositoryService>();

        services
            .AddScoped<IMapRepositoryService,
                MapRepositoryService>();

        services
            .AddScoped<IGameEventRepositoryService,
                GameEventRepositoryService>();

        services
            .AddScoped<ICtfFlagRepositoryService,
                CtfFlagRepositoryService>();

        services.AddScoped<IPowerupRepositoryService,
                PowerupRepositoryService>();
    }

    private static TService GetNonNullService<TService>(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetService<TService>() ??
               throw new Exception("Unable to discover critical service during startup. " +
                                   $"Service name: {typeof(TService).Name}");
    }
}