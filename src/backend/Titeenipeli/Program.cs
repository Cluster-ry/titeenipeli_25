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
using Titeenipeli.BackgroundServices;
using Titeenipeli.Context;
using Titeenipeli.Helpers;
using Titeenipeli.Middleware;
using Titeenipeli.Options;

namespace Titeenipeli;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<ApiDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

        JwtOptions jwtOptions = new JwtOptions();
        builder.Configuration.GetSection("JWT").Bind(jwtOptions);
        builder.Services.AddSingleton(jwtOptions);

        GameOptions gameOptions = new GameOptions();
        builder.Configuration.GetSection("Game").Bind(gameOptions);
        builder.Services.AddSingleton(gameOptions);

        // Adding OpenTelemetry tracing and metrics
        IOpenTelemetryBuilder otel = builder.Services.AddOpenTelemetry();

        string otelEnpoint = builder.Configuration["OpenTelemetryEndpoint"] ?? "localhost:4318";

        otel.ConfigureResource(resource => resource
            .AddService(builder.Environment.ApplicationName));

        otel.WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel");

            metrics.AddOtlpExporter((exporterOptions, metricReaderOptions) =>
            {
                exporterOptions.Endpoint = new Uri(otelEnpoint);
                exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
            });
        });

        otel.WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation();

            tracing.AddOtlpExporter(exporterOptions =>
            {
                exporterOptions.Endpoint = new Uri(otelEnpoint);
                exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
        });

        // Adding OpenTelemetry logging
        builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter((exporterOptions) =>
        {
            exporterOptions.Endpoint = new Uri(otelEnpoint);
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
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Encryption))
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
                   options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
               });

        AddBackgroundServices(builder.Services);

        WebApplication app = builder.Build();

        using (IServiceScope scope = app.Services.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;
            ApiDbContext dbContext = services.GetRequiredService<ApiDbContext>();

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

        // <snippet_UseWebSockets>
        WebSocketOptions webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromMinutes(2)
        };

        app.UseWebSockets(webSocketOptions);
        // </snippet_UseWebSockets>

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }

    private static void AddBackgroundServices(IServiceCollection services)
    {
        services.AddHostedService<PeriodicPrintToConsoleService>();
    }
}