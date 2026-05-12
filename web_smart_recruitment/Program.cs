using Microsoft.EntityFrameworkCore;
using web_smart_recruitment.Models;
using web_smart_recruitment.Services;
using web_smart_recruitment.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Thêm dịch vụ Controllers và Views
builder.Services.AddControllersWithViews();

// 2. Cấu hình Kết nối Cơ sở dữ liệu SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Đăng ký Dịch vụ Xác thực (Dependency Injection)
builder.Services.AddScoped<IAuthService, AuthService>();

// 4. Cấu hình cơ chế xác thực JWT Bearer (Hỗ trợ cho các thuộc tính [Authorize])
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
        };
    });

var app = builder.Build();

// Cấu hình Pipeline xử lý HTTP Request
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 5. QUAN TRỌNG: Thêm Middleware xử lý JWT từ Cookie TRƯỚC Authentication/Authorization
// Middleware này đảm bảo người dùng luôn được xác định danh tính từ Cookie của trình duyệt
app.UseMiddleware<JwtCookieMiddleware>();

app.UseAuthentication(); // Kích hoạt xác thực hệ thống
app.UseAuthorization();  // Kích hoạt phân quyền dựa trên Role trong Token

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
