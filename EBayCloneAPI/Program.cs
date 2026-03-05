
using System;
using EBayAPI.Configurations;
using EBayAPI.Models.Hooks;
using EBayCloneAPI.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace EBayCloneAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .WriteTo.File("logs/ebayclone_.txt",
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 10_000_000,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 7)
                .CreateLogger();

            builder.Host.UseSerilog();

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

            builder.Services
                .AddOptions<OrderCleanupSettings>()
                .Bind(builder.Configuration.GetSection("OrderCleanupSettings"))
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

            // Hosted cleanup service
            builder.Services.AddHostedService<EBayCloneAPI.Services.OrderCleanupHostedService>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            // ===== RUN DATABASE SEEDER =====
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                DbSeeder.SeedAsync(db).Wait();
            }
            app.UseSerilogRequestLogging();

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
