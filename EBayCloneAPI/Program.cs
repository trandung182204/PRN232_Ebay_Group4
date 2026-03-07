
using System;
using EBayAPI.Configurations;
using EBayAPI.Events;
using EBayAPI.Events.Handlers;
using EBayAPI.Models.Hooks;
using EBayCloneAPI.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace EBayCloneAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog early so startup logs go to file as well
            var runId = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}-{Environment.ProcessId}";
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                // Use a unique filename per run (timestamp + process id)
                .WriteTo.File(
                    path: $"logs/ebayclone-{runId}.txt",
                    rollingInterval: RollingInterval.Infinite,
                    fileSizeLimitBytes: null,
                    retainedFileCountLimit: null,
                    shared: true)
                .CreateLogger();

            builder.Host.UseSerilog();


            builder.Configuration
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

            // Add services to the container.

            builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });
            // CORS - allow frontend to call API during development
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });
            builder.Services.AddDbContext<EBayCloneAPI.Data.ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));
            // Session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Memory cache used by rate limiting middleware
            builder.Services.AddMemoryCache();

            builder.Services
                .AddOptions<OrderCleanupSettings>()
                .Bind(builder.Configuration.GetSection("OrderCleanupSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Email settings (bound from appsettings "EmailSettings")
            builder.Services
                .AddOptions<EmailSettings>()
                .Bind(builder.Configuration.GetSection("EmailSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Repositories
            builder.Services.AddScoped<EBayCloneAPI.Repositories.IUserRepository, EBayCloneAPI.Repositories.UserRepository>();
            builder.Services.AddScoped<EBayCloneAPI.Repositories.IProductRepository, EBayCloneAPI.Repositories.ProductRepository>();

            // Services
            builder.Services.AddScoped<EBayCloneAPI.Services.IEmailService, EBayCloneAPI.Services.EmailService>();
            builder.Services.AddScoped<EBayCloneAPI.Services.IPaymentService, EBayCloneAPI.Services.PaymentService>();
            builder.Services.AddScoped<EBayCloneAPI.Services.IShippingService, EBayCloneAPI.Services.ShippingService>();
            builder.Services.AddScoped<EBayCloneAPI.Services.IOrderService, EBayCloneAPI.Services.OrderService>();

            builder.Services.AddSingleton<PluginManager>();

            builder.Services.AddSingleton<IPaymentHook, StripePaymentPlugin>();
            builder.Services.AddSingleton<IShippingHook, VNPostShippingPlugin>();

            builder.Services.AddScoped<IPaymentEventHook, TransactionLogHook>();
            builder.Services.AddScoped<IShippingEventHook, ShippingLogHook>();

            // ── KAN-18: Event Bus (singleton; uses IServiceScopeFactory internally) ──
            builder.Services.AddSingleton<IEventBus, EventBus>();

            // ── Order Created: confirmation email handler (fires on order placement) ──
            builder.Services.AddScoped<IEventHandler<OrderCreatedEvent>, OrderCreatedEmailHandler>();

            // ── KAN-16: Payment confirmation email handler ──
            builder.Services.AddScoped<IEventHandler<OrderPaidEvent>, OrderPaidEmailHandler>();

            // ── KAN-17: Order status-change email handler ──
            builder.Services.AddScoped<IEventHandler<OrderStatusChangedEvent>, OrderStatusChangedEmailHandler>();

            // Hosted cleanup service
            builder.Services.AddHostedService<EBayCloneAPI.Services.OrderCleanupHostedService>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title   = "eBayClone API",
                    Version = "v1",
                    Description = "PRN232 Group 4 — eBay Clone REST API"
                });
                c.EnableAnnotations();
            });

            var app = builder.Build();
            // ===== RUN DATABASE SEEDER =====
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                DbSeeder.SeedAsync(db).Wait();
            }
            app.UseSerilogRequestLogging();

            // Apply IP rate limiting only to API endpoints
            app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), branch =>
            {
                branch.UseMiddleware<EBayCloneAPI.Middleware.IpRateLimitingMiddleware>();
            });

            // Configure the HTTP request pipeline.
            // Enable Swagger UI in all environments for testing
            app.UseSwagger();
            app.UseSwaggerUI();

            // Use plain HTTP for local testing
            // Note: HTTPS redirection removed to allow simple HTTP testing
            app.UseSession();

            // enable CORS
            app.UseCors("AllowAll");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
