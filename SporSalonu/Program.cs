using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.EntityFrameworkCore;
using YeniSalon.Data;
using YeniSalon.Models;
using YeniSalon.Services;

var builder = WebApplication.CreateBuilder(args);

// Api 
var openAIConfig = builder.Configuration.GetSection("OpenAI");


// Add services to the container.   
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddHttpClient();

// DbContext servisini ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity servislerini ekle
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;           // Rakam zorunluluðunu kaldýr
    options.Password.RequiredLength = 3;             // Minimum uzunluðu 3 yap
    options.Password.RequireNonAlphanumeric = false; // Özel karakter zorunluluðunu kaldýr
    options.Password.RequireUppercase = false;       // Büyük harf zorunluluðunu kaldýr
    options.Password.RequireLowercase = false;       // Küçük harf zorunluluðunu kaldýr

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// OpenAI servisini ekleme
builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddHttpClient<IOpenAIService, OpenAIService>();

// Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//APÝ ÝLE
app.MapControllerRoute(
    name: "antrenor-mvc",
    pattern: "AntrenorMVC/{action}/{id?}",
    defaults: new { controller = "AntrenorMVC" });

// API route'larý
app.MapControllers(); // API controller'larý için


// ADMÝN OLUÞTURMA
// Bu kodu app.MapControllerRoute(...); satýrýndan sonra, app.Run(); satýrýndan önce yapýþtýrýn.

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("=== SEED VERÝSÝ BAÞLATILIYOR ===");

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Veritabanýnýn hazýr olup olmadýðýný kontrol et (ÖNEMLÝ!)
        logger.LogInformation("Veritabanýna baðlanýlýyor...");
        await context.Database.EnsureCreatedAsync(); // Tablolar yoksa oluþturur
        logger.LogInformation("Veritabaný hazýr.");

        // Rolleri oluþtur
        string[] roller = { "Admin", "Uye" };
        foreach (var rol in roller)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                logger.LogInformation($"'{rol}' rolü oluþturuluyor...");
                await roleManager.CreateAsync(new IdentityRole(rol));
                logger.LogInformation($"'{rol}' rolü oluþturuldu.");
            }
        }

        // Admin kullanýcýsýný oluþtur
        var adminEmail = "b221210048@sakarya.edu.tr";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            logger.LogInformation($"Admin kullanýcýsý ({adminEmail}) oluþturuluyor...");
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Ad = "Admin",
                Soyad = "User",
                TCKimlikNo = "12345678901",
                DogumTarihi = new DateTime(1990, 1, 1),
                Cinsiyet = Cinsiyet.Erkek,
                Adres = "Sakarya Üniversitesi",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                AktifMi = true
            };

            // Þifreyi oluþtur
            var createResult = await userManager.CreateAsync(adminUser, "sau");

            if (createResult.Succeeded)
            {
                logger.LogInformation("Admin kullanýcýsý baþarýyla oluþturuldu. Role atanýyor...");
                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleResult.Succeeded)
                {
                    logger.LogInformation($"Admin kullanýcýsý 'Admin' rolüne atandý. ID: {adminUser.Id}");
                }
                else
                {
                    // Role atama hatasý
                    logger.LogError($"Role atama baþarýsýz: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Kullanýcý oluþturma hatasý
                logger.LogError($"Kullanýcý oluþturma baþarýsýz: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            logger.LogInformation($"Admin kullanýcýsý ({adminEmail}) zaten mevcut. ID: {adminUser.Id}");

            // Kullanýcý varsa þifresini güncelle (opsiyonel)
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "sau");
            await userManager.UpdateAsync(adminUser);
            logger.LogInformation("Admin þifresi güncellendi (sau).");
        }

        logger.LogInformation("=== SEED VERÝSÝ TAMAMLANDI ===");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "!!! SEED VERÝSÝ SIRASINDA KRÝTÝK BÝR HATA OLUÞTU !!!");
    }
}


app.Run();