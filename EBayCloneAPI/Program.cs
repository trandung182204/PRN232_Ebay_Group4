
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace EBayCloneAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
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

            // Repositories
            builder.Services.AddScoped<EBayCloneAPI.Repositories.IUserRepository, EBayCloneAPI.Repositories.UserRepository>();
            builder.Services.AddScoped<EBayCloneAPI.Repositories.IProductRepository, EBayCloneAPI.Repositories.ProductRepository>();

            // Services
            builder.Services.AddScoped<EBayCloneAPI.Services.IEmailService, EBayCloneAPI.Services.EmailService>();
            builder.Services.AddScoped<EBayCloneAPI.Services.IPaymentService, EBayCloneAPI.Services.PaymentService>();
            builder.Services.AddScoped<EBayCloneAPI.Services.IShippingService, EBayCloneAPI.Services.ShippingService>();
            builder.Services.AddScoped<EBayCloneAPI.Services.IOrderService, EBayCloneAPI.Services.OrderService>();

            // Hosted cleanup service
            builder.Services.AddHostedService<EBayCloneAPI.Services.OrderCleanupHostedService>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

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
