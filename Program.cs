using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SadrokartonyTrmota.Components;
using SadrokartonyTrmota.Components.Account;
using SadrokartonyTrmota.Data;
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

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
                .AddIdentityCookies();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.Run();
        }
    }
}
