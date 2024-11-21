using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Controllers;
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
            services.AddControllersWithViews();
            services.AddSingleton<CloudinaryService>();
            services.AddSingleton<MongoDBService>();
            services.AddScoped<ForgotPasswordService>();
            services.AddHttpContextAccessor();

            // Cấu hình session
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                // Nếu không chỉ định, tên cookie mặc định sẽ là .AspNetCore.Session
                options.Cookie.Name = ".AspBookCore.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(30);  // Thời gian hết hạn session
                options.Cookie.IsEssential = true;  // Cookie bắt buộc
            });

            // Cấu hình các dịch vụ liên quan đến tài khoản người dùng và dữ liệu của ứng dụng
            services.AddScoped<MongoDBService>();  // Thêm MongoDB service cho dự án
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSession();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

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

