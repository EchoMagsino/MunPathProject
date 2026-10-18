# MunPath Registration & Email OTP Verification Guide

This step-by-step guide details how `AuthService` registers new student accounts, securely hashes credentials and OTP codes, and sends verification emails using Gmail SMTP and MailKit.

---

## 1. Prerequisites & Environment Setup

Before implementing the registration logic, ensure required packages are installed and secrets are properly configured.

### Step 1.1: Install Required Packages

Navigate to your project directory containing `MunPath.csproj` and run:

```bash
dotnet add package MailKit
dotnet add package MimeKit
dotnet add package Microsoft.EntityFrameworkCore
```

### Step 1.2: Configure User Secrets

Initialize User Secrets and set your SMTP and HMAC security keys:

```bash
dotnet user-secrets init

# Email SMTP Credentials
dotnet user-secrets set "EmailSettings:Email" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:AppPassword" "your-app-password"

# OTP Hashing Key (Minimum 32 random characters recommended)
dotnet user-secrets set "Otp:HashKey" "YourSuperSecretCryptoHashKey12345!"
```

> **Security Note:** Never hardcode credentials or secrets into source code or commit them to version control.

---

## 2. Gmail SMTP Setup

To allow MunPath to send automated emails via Gmail:

1. Log in to your primary Google Account.
2. Ensure **2-Step Verification** is enabled.
3. Visit [Google App Passwords](https://myaccount.google.com/apppasswords).
4. Create an App Password for **MunPath**.
5. Copy the generated 16-character password and assign it to `EmailSettings:AppPassword` in User Secrets.

### Reference SMTP Configuration

| Setting | Value |
|---|---|
| **Host** | `smtp.gmail.com` |
| **Port** | `587` |
| **Encryption** | `STARTTLS` |
| **Authentication** | Gmail Address + App Password |

---

## 3. Required Namespaces

Ensure `AuthService.cs` imports all necessary dependencies:

```csharp
using System.Security.Cryptography;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MunPath.Data;
using MunPath.Models;
using MunPath.Models.DTOs;
using MunPath.Models.EmailVerification;
```

---

## 4. AuthService Registration and OTP Implementation

This section details the end-to-end flow inside `RegisterAsync()`.

### Step 4.1: Inject Required Dependencies

Inject the database context, password hasher, and configuration instance into `AuthService`:

```csharp
public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<UserModel> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        ApplicationDbContext context,
        PasswordHasher<UserModel> passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }
}
```

- **`ApplicationDbContext`**: Handles database interactions for users, roles, and OTP records.
- **`PasswordHasher<UserModel>`**: Provides secure PBKDF2 password hashing.
- **`IConfiguration`**: Retrieves secrets from `appsettings.json` or User Secrets.

---

### Step 4.2: Verify Email Uniqueness

Check if the requested email is already registered in the system:

```csharp
bool emailExist = await _context.Users
    .AnyAsync(u => u.Email == request.Email);

if (emailExist)
{
    throw new InvalidOperationException("This email is already registered.");
}
```

---

### Step 4.3: Retrieve Default Student Role

Fetch the `Student` role entity to assign to the new user:

```csharp
var userRole = await _context.Roles
    .FirstOrDefaultAsync(r => r.RoleName == "Student");

if (userRole == null)
{
    throw new InvalidOperationException("The default user role ('Student') was not found in the database.");
}
```

---

### Step 4.4: Create and Save User Record

Map the incoming request payload to a `UserModel`, hash the password, and persist the entity:

```csharp
var user = SignUpMapper.ToUserModel(request);

// Hash the plain-text password securely
user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

// Assign role association
user.RoleName = userRole.RoleName;
user.Role = userRole;

_context.Users.Add(user);

// Save immediately so EF Core generates the user's primary key ID
await _context.SaveChangesAsync();
```

---

### Step 4.5: Generate Cryptographic OTP and Hash

Generate a cryptographically secure 6-digit string and hash it using HMAC-SHA256:

```csharp
// 1. Generate secure random 6-digit string (e.g., "042918")
string otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

// 2. Fetch secret key
string hashKey = _configuration["Otp:HashKey"] 
    ?? throw new InvalidOperationException("Otp:HashKey is not configured.");

// 3. Generate HMAC-SHA256 hash of the OTP
string otpHash = Convert.ToHexString(
    HMACSHA256.HashData(
        Encoding.UTF8.GetBytes(hashKey),
        Encoding.UTF8.GetBytes(otp)
    )
);
```

---

### Step 4.6: Store Email Verification Record

Link the generated OTP hash to the newly created user in the database:

```csharp
var verification = new EmailVerificationModel
{
    UserId = user.Id,
    OtpHash = otpHash,
    CreatedAtUtc = DateTime.UtcNow,
    ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10), // Expires in 10 minutes
    IsUsed = false,
    Attempts = 0
};

_context.EmailVerifications.Add(verification);
await _context.SaveChangesAsync();
```

---

### Step 4.7: Send Verification Email via MailKit

Construct and dispatch the message using MimeKit and MailKit SMTP:

```csharp
var senderEmail = _configuration["EmailSettings:Email"]
    ?? throw new InvalidOperationException("EmailSettings:Email is missing.");

var appPassword = _configuration["EmailSettings:AppPassword"]
    ?? throw new InvalidOperationException("EmailSettings:AppPassword is missing.");

var message = new MimeMessage();
message.From.Add(new MailboxAddress("MunPath", senderEmail));
message.To.Add(MailboxAddress.Parse(request.Email));
message.Subject = "Your MunPath Verification Code";

message.Body = new TextPart("plain")
{
    Text = $"Your verification code is: {otp}\n\nThis code will expire in 10 minutes. If you did not request this code, please ignore this email."
};

using var smtp = new SmtpClient();

// Connect via STARTTLS on Port 587
await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

// Authenticate and send
await smtp.AuthenticateAsync(senderEmail, appPassword);
await smtp.SendAsync(message);
await smtp.DisconnectAsync(true);
```

---

## 5. End-to-End Registration Flow Summary

```
[ User Request ]
       │
       ▼
1. Validate Unique Email
       │
       ▼
2. Fetch Default "Student" Role
       │
       ▼
3. Hash Password & Create User ──► [ DB: Save User (Get UserId) ]
       │
       ▼
4. Generate 6-Digit OTP & HMAC-SHA256 Hash
       │
       ▼
5. Save OTP Record ──────────────► [ DB: Save EmailVerificationModel ]
       │
       ▼
6. Send Plaintext OTP via MailKit ──► [ User Email Inbox ]
       │
       ▼
7. Return Success Response
```

---

## 6. Testing & Validation Checklist

1. **Start Application**: Run `dotnet watch run`.
2. **Submit Registration**: Post valid payload data via form or endpoint.
3. **Inbox Check**: Confirm delivery in the recipient inbox and spam folder.
4. **Database Verification**:
   - Verify user record exists in `Users` table.
   - Verify that `EmailVerifications` contains the corresponding `UserId`, `OtpHash`, `IsUsed = false`, and accurate `ExpiresAtUtc`.
5. **OTP Verification Logic**: Ensure your `VerifyEmailAsync()` logic handles:
   - Comparing HMAC-SHA256 hash of the input OTP against `OtpHash`.
   - Rejection if `DateTime.UtcNow > ExpiresAtUtc`.
   - Rejection if `IsUsed == true`.
   - Incrementing `Attempts` and enforcing max-attempt locks.