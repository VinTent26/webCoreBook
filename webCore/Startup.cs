using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using webCore.Services;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;

namespace webCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add controllers and views
            services.AddControllersWithViews();

            // Add MongoDB Client (singleton because it is thread-safe)
            services.AddSingleton<IMongoClient>(sp =>
                new MongoClient(Configuration.GetConnectionString("MongoDBConnection")));

            // Add Cloudinary service for image upload (singleton)
            services.AddSingleton<CloudinaryService>();

            // Register MongoDBService with DI container
            services.AddScoped<MongoDBService>();  // MongoDBService will automatically receive IConfiguration through DI

            // Access HTTP context for session management
            services.AddHttpContextAccessor();

            // Session configuration
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = ".AspBookCore.Session";  // Session cookie name
                options.IdleTimeout = TimeSpan.FromMinutes(30);  // Session expiration time
                options.Cookie.IsEssential = true;  // Cookie is required for the session
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Use session
            app.UseSession();

            // Configure error handling and environment settings
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Enable HTTPS redirection and static file serving
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Routing configuration
            app.UseRouting();

            // Authorization middleware (if needed)
            app.UseAuthorization();

            // Configure endpoints for the application
            app.UseEndpoints(endpoints =>
            {
                // Default route
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Custom route for DetailUserController
                endpoints.MapControllerRoute(
                    name: "detailUser",
                    pattern: "DetailUser/{action=Index}/{id?}");
            });
        }
    }
}
