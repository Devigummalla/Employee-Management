using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using EmployeeManagement.Data;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
var builder = WebApplication.CreateBuilder(args);

// 1. Register MVC services and Microsoft Identity UI along with global anti-forgeryfilter
builder.Services
    .AddControllersWithViews(options =>
    {
        options.Filters.Add(
            new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    })
    .AddMicrosoftIdentityUI();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
// 2. Configure Microsoft Entra authentication
builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(
        builder.Configuration.GetSection("AzureAd"));
//3.Configure cookie settings for security
builder.Services.Configure<CookieAuthenticationOptions>(
    CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.Cookie.HttpOnly = true;

        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;

        options.Cookie.SameSite = SameSiteMode.Lax;
    });
// 4. Configure authorization
builder.Services.AddAuthorization(options => { 
    options.AddPolicy("CanManageEmployees", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin");
    });

    options.AddPolicy("CanViewEmployees", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin", "Employee");
    });
});
// empoyee service dependency injection
builder.Services.AddScoped<EmployeeProfileService>();
//encrypt-decrypt dependemcy injection
builder.Services.AddScoped<AesGcmEncryptionService>();
var app = builder.Build();

// 5. Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Authentication must come before authorization
app.UseAuthentication();

app.UseAuthorization();

// Map Microsoft Identity UI endpoints
app.MapControllers();

// Map MVC controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();