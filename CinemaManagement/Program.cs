using CinemaManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using CinemaManagement.Services;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using CinemaManagement.Areas.Identity.Admin.Repository;

var builder = WebApplication.CreateBuilder(args);

//Add Email Sender
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<EmailService>();

// Cấu hình Kestrel để lắng nghe trên tất cả IP
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5200); // Chạy HTTP, cho phép truy cập từ bên ngoài
});

// Cấu hình Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Cấu hình Cookie Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cấu hình localization mặc định
var supportedCultures = new[] { new CultureInfo("en-US") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// ✅ BỔ SUNG cấu hình CultureInfo mặc định cho toàn bộ app
var cultureInfo = new CultureInfo("en-US");
cultureInfo.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
cultureInfo.DateTimeFormat.LongTimePattern = "HH:mm";
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Khởi tạo dịch vụ Momo
builder.Services.AddSingleton<MomoService>();

// Cấu hình CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins("https://5e93-112-197-18-138.ngrok-free.app")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Dịch vụ reset ghế
builder.Services.AddHostedService<SeatResetService>();

// Đăng ký HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Razor Pages và Controllers
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Middleware lỗi
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection(); // ❌ Không cần dùng HTTPS ở localhost
app.UseStaticFiles();

// ✅ Áp dụng localization phải nằm trước UseRouting
app.UseRequestLocalization();

app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);

// ✅ Session + Auth
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Route mặc định
app.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Movies}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "Identity",
    areaName: "Identity",
    pattern: "Account/{controller=Manage}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();

