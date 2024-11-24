using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using webCore.Services;
using Microsoft.AspNetCore.Http;
using webCore.MongoHelper;

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
            // Thêm các dịch vụ cần thiết cho ứng dụng
            services.AddControllersWithViews();

            // Dịch vụ Cloudinary cho việc upload ảnh
            services.AddSingleton<CloudinaryService>();

            // Dịch vụ MongoDB để làm việc với cơ sở dữ liệu
            services.AddSingleton<MongoDBService>();

            // Truy cập thông tin từ HttpContext
            services.AddHttpContextAccessor();

            services.AddSingleton<CloudinaryService>();  // Dịch vụ Cloudinary cho việc upload ảnh
            services.AddSingleton<MongoDBService>();     // Dịch vụ MongoDB để làm việc với cơ sở dữ liệu
            services.AddHttpContextAccessor();          // Truy cập thông tin từ HttpContext
            services.AddScoped<DetailProductService>();
           
            // Cấu hình session
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = ".AspBookCore.Session";  // Tên cookie session
                options.IdleTimeout = TimeSpan.FromMinutes(30);  // Thời gian hết hạn session
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
            // Cấu hình session
            app.UseSession();

            // Cấu hình các trang lỗi và môi trường phát triển
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Chuyển hướng và sử dụng các file tĩnh
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Cấu hình routing
            app.UseRouting();

            // Cấu hình ủy quyền
            app.UseAuthorization();

            // Định nghĩa các endpoint cho ứng dụng
            app.UseEndpoints(endpoints =>
            {
                // Route mặc định
                endpoints.MapControllerRoute(
                 name: "default",
                 pattern: "{controller=Home}/{action=Index}/{id?}");

                // Route riêng cho DetailController
                endpoints.MapControllerRoute(
                    name: "detailUser", // Tên route tùy chỉnh
                    pattern: "DetailUser/{action=Index}/{id?}"); // Truy cập DetailController qua /DetailUser
            });
        }
    }
}
