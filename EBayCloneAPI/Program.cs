using EBayAPI.Services;
using EBayCloneAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace EBayAPI
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
            builder.Services.AddScoped<EBayCloneAPI.Services.IPaymentService, PaymentService>();
            builder.Services.AddScoped<EBayCloneAPI.Services.IShippingService, EBayCloneAPI.Services.ShippingService>();
            builder.Services.AddScoped<EBayCloneAPI.Services.IOrderService, EBayCloneAPI.Services.OrderService>();
            builder.Services.AddScoped<IPaymentProvider, CodPaymentProvider>();
            builder.Services.AddScoped<IPaymentProvider, PaypalPaymentProvider>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddHttpClient<PaypalService>();
            // Hosted cleanup service
            /*
            builder.Services.AddHostedService<EBayCloneAPI.Services.OrderCleanupHostedService>();
            */
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "EBay Clone API",
                    Version = "v1"
                });

                // 👉 Add X-PAYMENT-KEY
                c.AddSecurityDefinition("PaymentKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "X-PAYMENT-KEY",
                    Type = SecuritySchemeType.ApiKey
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "PaymentKey"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            var app = builder.Build();
            app.UseMiddleware<PaymentAuthMiddleware>();
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
