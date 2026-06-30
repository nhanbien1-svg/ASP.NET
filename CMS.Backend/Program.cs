using Microsoft.EntityFrameworkCore;
using CMS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json.Serialization; // Xử lý JSON
using System.Reflection;
using System.IO;
using CMS.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 0. CẤU HÌNH EMAIL ---
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

// Đăng ký AIChatService
builder.Services.AddHttpClient<AIChatService>();

// --- 1. CẤU HÌNH CORS (BẬT ĐÈN XANH CHO REACT) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
});

// --- 2. KẾT NỐI CƠ SỞ DỮ LIỆU ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 3. CẤU HÌNH XÁC THỰC (DÀNH CHO ADMIN MVC) ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

// --- 4. FIX LỖI VÒNG LẶP JSON & ĐĂNG KÝ CONTROLLER ---
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Ngắt vòng lặp vô tận giữa các thực thể (VD: Post gọi Category, Category lại gọi Post)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// --- 5. ĐĂNG KÝ SWAGGER (MỚI BỔ SUNG) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Bật tính năng đọc chú thích XML để hiển thị trên UI Swagger
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

// ====================================================================
// CẤU HÌNH HTTP REQUEST PIPELINE (THỨ TỰ BẮT BUỘC PHẢI CHUẨN)
// ====================================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // MỚI BỔ SUNG: Kích hoạt giao diện Swagger khi code ở môi trường Development
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Bước 1: Phục vụ file tĩnh (Ảnh, CSS, JS...). ASP.NET tự hiểu là thư mục wwwroot
app.UseStaticFiles();

// Bước 2: Routing (Bắt buộc phải gọi trước CORS)
app.UseRouting();

// Bước 3: CORS (Bắt buộc gọi SAU Routing và TRƯỚC Auth)
app.UseCors("AllowReactApp");

// Bước 4: Xác thực & Phân quyền
app.UseAuthentication();
app.UseAuthorization();

// Bước 5: Ánh xạ Controller (Mapping)
app.MapControllers(); // Dành cho các API Controller của React ([ApiController])
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // Dành cho Admin MVC

app.Run();