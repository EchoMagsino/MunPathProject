using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using MunPath.Extensions;

// Read local secrets before the default configuration providers load. Existing
// machine or launch-profile variables take precedence over .env values.
LoadDotEnv(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.ApplicationService(builder.Configuration);

// Configure Cookie Authentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Path to redirect unauthenticated users
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        // Cookie lifetime options
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;

        // Security settings
        options.Cookie.Name = "MunPath.Auth";
        options.Cookie.HttpOnly = true; // Protects against XSS
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
        options.Cookie.SameSite = SameSiteMode.Lax; // Basic CSRF protection
    });

builder.Services.AddAuthorization();
builder.Services.AddOptions();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Static files should generally come before routing if serving standard assets, 
// but right after routing is also fine. Let's place it cleanly.
app.UseRouting();

app.UseStaticFiles();

// Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map routes and static assets cleanly without duplication
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static void LoadDotEnv(string path)
{
    if (!File.Exists(path))
    {
        return;
    }

    foreach (var rawLine in File.ReadLines(path))
    {
        var line = rawLine.Trim();
        if (line.Length == 0 || line.StartsWith('#'))
        {
            continue;
        }

        if (line.StartsWith("export ", StringComparison.Ordinal))
        {
            line = line[7..].TrimStart();
        }

        var separator = line.IndexOf('=');
        if (separator <= 0)
        {
            continue;
        }

        var key = line[..separator].Trim();
        var value = line[(separator + 1)..].Trim();
        if (value.Length >= 2 &&
            ((value[0] == '"' && value[^1] == '"') ||
             (value[0] == '\'' && value[^1] == '\'')))
        {
            value = value[1..^1];
        }

        // Keep values already supplied by the host, shell, or launch profile.
        if (Environment.GetEnvironmentVariable(key) is null)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}