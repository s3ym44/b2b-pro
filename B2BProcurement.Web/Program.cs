using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.HttpOverrides;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// Railway/Production PORT Configuration
// ========================================
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// ========================================
// 1. Configuration
// ========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// SQLite için veritabanı yolunu ContentRootPath'e göre ayarla
if (connectionString.Contains("Data Source=") && !connectionString.Contains("/"))
{
    var dbFileName = connectionString.Replace("Data Source=", "").Replace("./", "").Trim();
    var dbPath = Path.Combine(builder.Environment.ContentRootPath, dbFileName);
    connectionString = $"Data Source={dbPath}";
}

// ========================================
// 2. Database Context
// ========================================
builder.Services.AddDbContext<B2BProcurement.Data.Context.ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// ========================================
// 3. Repository Registrations
// ========================================
builder.Services.AddScoped(typeof(B2BProcurement.Data.Repositories.IRepository<>), 
    typeof(B2BProcurement.Data.Repositories.Repository<>));
builder.Services.AddScoped<B2BProcurement.Data.Repositories.IRfqRepository, 
    B2BProcurement.Data.Repositories.RfqRepository>();
builder.Services.AddScoped<B2BProcurement.Data.Repositories.IQuotationRepository, 
    B2BProcurement.Data.Repositories.QuotationRepository>();

// ========================================
// 4. Service Registrations
// ========================================
builder.Services.AddScoped<B2BProcurement.Business.Interfaces.IAuthService, 
    B2BProcurement.Business.Services.AuthService>();
builder.Services.AddScoped<B2BProcurement.Business.Interfaces.ICompanyService, 
    B2BProcurement.Business.Services.CompanyService>();
builder.Services.AddScoped<B2BProcurement.Business.Interfaces.IMaterialService, 
    B2BProcurement.Business.Services.MaterialService>();
builder.Services.AddScoped<B2BProcurement.Business.Interfaces.ISupplierService, 
    B2BProcurement.Business.Services.SupplierService>();
builder.Services.AddScoped<B2BProcurement.Business.Interfaces.IRfqService, 
    B2BProcurement.Business.Services.RfqService>();
builder.Services.AddScoped<B2BProcurement.Business.Interfaces.IQuotationService, 
    B2BProcurement.Business.Services.QuotationService>();
builder.Services.AddScoped<B2BProcurement.Business.Interfaces.INotificationService, 
    B2BProcurement.Business.Services.NotificationService>();
builder.Services.AddScoped<B2BProcurement.Business.Interfaces.IEmailService, 
    B2BProcurement.Business.Services.EmailService>();

// SMTP Settings
builder.Services.Configure<B2BProcurement.Business.Interfaces.SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));

// SignalR
builder.Services.AddSignalR();

// ========================================
// 5. AutoMapper
// ========================================
builder.Services.AddAutoMapper(typeof(B2BProcurement.Business.Mappings.MappingProfile));

// ========================================
// 6. Authentication - Cookie
// ========================================
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Name = "B2BProcurement.Auth";
    });

// ========================================
// 7. Session Configuration
// ========================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "B2BProcurement.Session";
});

// ========================================
// 8. Localization
// ========================================
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// ========================================
// Forwarded Headers (for Railway/Proxy)
// ========================================
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Health Checks
builder.Services.AddHealthChecks();

builder.Services.AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(B2BProcurement.Resources.SharedResource));
    });

var app = builder.Build();

// ========================================
// Database Initialization & Seed Data
// ========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<B2BProcurement.Data.Context.ApplicationDbContext>();
        
        // Veritabanını oluştur (migration olmadan)
        await context.Database.EnsureCreatedAsync();
        
        // Seed data yükle
        await B2BProcurement.Data.Seed.DataSeeder.SeedAsync(context);
        
        Console.WriteLine("Database initialized and seed data loaded successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

// ========================================
// 9. Middleware Pipeline
// ========================================

// Forwarded Headers (must be first for Railway/Proxy)
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // HSTS is handled by Railway
}

// Skip HTTPS redirect in production (Railway handles SSL termination)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// ========================================
// Localization Middleware
// ========================================
var supportedCultures = new[] { new CultureInfo("tr-TR"), new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr-TR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider { CookieName = "B2BProcurement.Culture" },
        new QueryStringRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    }
});

// ========================================
// 10. Endpoints
// ========================================
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// SignalR Hub
app.MapHub<B2BProcurement.Business.Hubs.NotificationHub>("/notificationHub");

// Health Check Endpoint (for Railway)
app.MapHealthChecks("/health");

app.Run();
