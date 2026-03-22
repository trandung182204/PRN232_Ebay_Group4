using System;
using EbayCloneWeb.Services;

namespace EbayCloneWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName, (sp, client) =>
            {
                var config = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                var env = sp.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>();

                var apiBaseUrl = config["ApiSettings:BaseUrl"];

                if (string.IsNullOrWhiteSpace(apiBaseUrl))
                {
                    // Default to localhost in Development for local dev, otherwise use nginx reverse-proxy host inside Docker
                    apiBaseUrl = env.IsDevelopment() ? "http://localhost:5174" : "http://nginx";
                }

                client.BaseAddress = new Uri(apiBaseUrl);
            });
            builder.Services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            // Product service not registered; pages use HttpClient directly

            var app = builder.Build();

            app.UseForwardedHeaders(new Microsoft.AspNetCore.Builder.ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            });

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
