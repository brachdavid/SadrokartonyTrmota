using SadrokartonyTrmota.Components;
using System.Security.Cryptography;

namespace SadrokartonyTrmota
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddBlazorBootstrap();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/"); // Migrations endpoint removed
            }
            else
            {
                app.UseExceptionHandler("/");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            // Middleware pro generování nonce a pøidání CSP hlavièky
            app.Use(async (context, next) =>
            {
                var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
                context.Items["CSPNonce"] = nonce;

                context.Response.Headers.Append("Content-Security-Policy",
                    $"default-src 'self'; " +
                    $"script-src 'self' 'nonce-{nonce}' https://cdn.jsdelivr.net; " +
                    $"style-src 'self' https://cdn.jsdelivr.net 'unsafe-inline'; " +
                    $"img-src 'self' data: https://www.firmy.cz https://toplist.cz; " +
                    $"connect-src 'self' ws://localhost:* wss://localhost:* http://localhost:* https://localhost:* " +
                    $"https://www.sadrokartonytrmota.cz ws://www.sadrokartonytrmota.cz wss://www.sadrokartonytrmota.cz; " +
                    $"font-src 'self' https://cdn.jsdelivr.net; " +
                    $"frame-src 'self' https://maps.google.com https://www.google.com; " +
                    $"object-src 'none'; " +
                    $"base-uri 'self'; " +
                    $"frame-ancestors 'self';");

                await next();
            });

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
