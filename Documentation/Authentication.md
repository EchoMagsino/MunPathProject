# ASP.NET Core Identity & Cookie Authentication Setup Guide (Traditional API Endpoint)

This document contains the full setup instructions and code for configuring ASP.NET Core Identity Core with Entity Framework Core and custom Cookie Authentication in your project.

---

## 1. Create the User Model
Define your application user model by extending `IdentityUser`.

```csharp
using Microsoft.AspNetCore.Identity;

namespace MunPathProject.Models
{
    public class User : IdentityUser
    {
        // Add custom profile fields here (e.g., FirstName, LastName)
    }
}
```

---

## 2. Configure ApplicationDbContext
Ensure your database context inherits from `IdentityDbContext<User>` to include Identity tables (Users, Roles, Claims, etc.).

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MunPathProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add additional DbSets for your application domain models here
    }
}
```

---

## 3. Configure `Program.cs`
Register the services, Identity stores, and Cookie authentication middleware in `Program.cs`.

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MunPathProject.Data;
using MunPathProject.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Add DbContext with your connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Configure Identity Core with EF Stores
builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddApiEndpoints();

// 3. Configure Authentication & Cookies
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;

        options.Cookie.Name = "MunPath.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 4. Configure the HTTP Request Pipeline
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

app.Run();
```

---

## 4. Secure Action Methods
Always use `[ValidateAntiForgeryToken]` on form POST actions when using Cookie Authentication to protect against CSRF attacks.

```csharp
using Microsoft.AspNetCore.Mvc;

namespace MunPathProject.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Perform login logic here

            return RedirectToAction("Index", "Home");
        }
    }
}




//=========================================================================================================================================================//



# ASP.NET Core Identity & Cookie Authentication Setup Guide (Standard ASP.NET Core MVC application with Controllers and Views)
```

This guide covers setting up simple, custom **Cookie-based Authentication** for ASP.NET Core MVC apps that use **Controllers and Views** (`AddControllersWithViews`).

Unlike full ASP.NET Core Identity setups, this lightweight approach gives you direct control over your database queries and handles user sessions via encrypted cookies.

---

## 1. Configure `Program.cs`

In your `Program.cs`, add the Cookie Authentication services and configure the middleware pipeline.

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. Add MVC Controllers with Views
builder.Services.AddControllersWithViews();

// 2. Configure Cookie Authentication
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

var app = builder.Build();

// 3. Configure HTTP Request Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

---

## 2. Implement the Account Controller

Create an `AccountController.cs` file to handle **Login**, **Logout**, and **Access Denied** routes.

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MunPathProject.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            // If user is already authenticated, redirect to home page
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against CSRF attacks
        public async Task<IActionResult> Login(string username, string password)
        {
            // 1. Check user credentials against your custom database table here
            // Example stub:
            bool isValidUser = (username == "admin" && password == "password123");

            if (isValidUser)
            {
                // 2. Create claims for the logged-in user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Admin") // Optional: Assign roles
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true // Keeps cookie saved across browser restarts
                };

                // 3. Issue the encrypted auth cookie to the browser
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Deletes the authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
```

---

## 3. Protect Controllers & Views with `[Authorize]`

To protect pages and endpoints so only authenticated users can visit them, decorate your controllers or actions with `[Authorize]`.

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MunPathProject.Controllers
{
    [Authorize] // Requires user to be logged in for all actions in this controller
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // You can access logged-in user details via User.Identity
            string username = User.Identity?.Name ?? "Guest";
            ViewBag.Username = username;

            return View();
        }

        [Authorize(Roles = "Admin")] // Requires specific role
        public IActionResult AdminDashboard()
        {
            return View();
        }
    }
}
```

---

## 4. Key Takeaways

1. **`[ValidateAntiForgeryToken]`**: Always use this attribute on `HttpPost` actions processing form data to prevent Cross-Site Request Forgery (CSRF).
2. **`HttpOnly = true`**: Ensures JavaScript running on the client cannot read the authentication cookie, preventing XSS-based token theft.
3. **`HttpContext.SignInAsync`**: Encrypts claims into a ticket and stores it inside the `MunPath.Auth` browser cookie.