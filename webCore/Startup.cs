using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using webCore.MongoHelper;
using webCore.Services;
using Microsoft.AspNetCore.Http;

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
            {
                var logger = sp.GetRequiredService<ILogger<Startup>>();

                // Read MongoDB configuration
                var mongoConfig = Configuration.GetSection("MongoDB");
                var mongoConnection = mongoConfig["ConnectionString"];
                var databaseName = mongoConfig["DatabaseName"];

                // Log and validate the connection string
                if (string.IsNullOrWhiteSpace(mongoConnection))
                {
                    logger.LogError("MongoDB connection string is missing or empty.");
                    throw new InvalidOperationException("MongoDB connection string is not configured.");
                }

                logger.LogInformation("MongoDB connection string is configured.");
                return new MongoClient(mongoConnection);
            });

            // Add Cloudinary service for image upload (singleton)
            services.AddSingleton<CloudinaryService>();

            // Register MongoDBService with DI container
            services.AddScoped<MongoDBService>();

            // Access HTTP context for session management
            services.AddHttpContextAccessor();
            services.AddScoped<ProductService>();
            services.AddScoped<CategoryService>();
            // Session configuration

            services.AddSingleton<CloudinaryService>();  // Dịch vụ Cloudinary cho việc upload ảnh
            services.AddSingleton<MongoDBService>();     // Dịch vụ MongoDB để làm việc với cơ sở dữ liệu
            services.AddHttpContextAccessor();          // Truy cập thông tin từ HttpContext
            services.AddScoped<DetailProductService>();
            services.AddScoped<CartService>();
            services.AddScoped<OrderService>();
            services.AddScoped<VoucherClientService>();
            services.AddScoped<UserService>();
            services.AddControllersWithViews(options =>
            {
                // Đăng ký Action Filter toàn cục
                options.Filters.Add<SetLoginStatusFilter>();
            });
            services.AddScoped<SetLoginStatusFilter>();
            // Cấu hình session
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = ".AspBookCore.Session"; // Session cookie name
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Session expiration time
                options.Cookie.IsEssential = true; // Cookie is required for the session
                options.Cookie.Name = ".AspBookCore.Session";  // Tên cookie session
                options.IdleTimeout = TimeSpan.FromMinutes(30);  // Thời gian hết hạn session
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.IsEssential = true;  // Cookie bắt buộc
            });
            // Cấu hình JSON options trong ConfigureServices
            services.AddControllers().AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

            // Cấu hình các dịch vụ liên quan đến tài khoản người dùng và dữ liệu của ứng dụng
            services.AddScoped<MongoDBService>();  // Thêm MongoDB service cho dự án
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
