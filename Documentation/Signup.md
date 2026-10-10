## AuthService Registration and OTP Implementation

This section explains how `AuthService` registers a new student account and prepares an email verification code.

### 1. Inject Required Dependencies

In `AuthService.cs`, inject the database context, password hasher, and configuration:

```csharp
public AuthService(
    ApplicationDbContext context,
    PasswordHasher<UserModel> passwordHasher,
    IConfiguration configuration)
{
    _context = context;
    _passwordHasher = passwordHasher;
    _configuration = configuration;
}
```

- `ApplicationDbContext` accesses users, roles, and email verification records.
- `PasswordHasher<UserModel>` securely hashes the user's password.
- `IConfiguration` reads secrets from User Secrets or other configuration providers.

### 2. Check for Duplicate Emails

Before creating an account, check whether the email is already registered:

```csharp
bool emailExist = await _context.Users
    .AnyAsync(u => u.Email == request.Email);

if (emailExist)
{
    throw new InvalidOperationException(
        "This email is already registered");
}
```

This prevents registration when a user with the same email already exists.

### 3. Assign the Student Role

Retrieve the default `Student` role from the database:

```csharp
var userRole = await _context.Roles
    .FirstOrDefaultAsync(r => r.RoleName == "Student");

if (userRole == null)
{
    throw new InvalidOperationException(
        "The default user role was not found.");
}
```

The `Student` role must already exist in the Roles table.

### 4. Create the User Account

Map the signup request to a user model, hash the password, and assign the role:

```csharp
var user = SignUpMapper.ToUserModel(request);

user.PasswordHash = _passwordHasher.HashPassword(
    user, request.Password);

user.RoleName = userRole.RoleName;
user.Role = userRole;

_context.Users.Add(user);
await _context.SaveChangesAsync();
```

Saving the user first ensures the database generates the user's ID, which is needed for the verification record.

### 5. Generate and Hash the OTP

Generate a random six-digit verification code:

```csharp
string otp = RandomNumberGenerator
    .GetInt32(0, 1_000_000)
    .ToString("D6");
```

Hash the OTP using HMAC-SHA256:

```csharp
string otpHash = Convert.ToHexString(
    HMACSHA256.HashData(
        Encoding.UTF8.GetBytes(
            _configuration["Otp:HashKey"]!),
        Encoding.UTF8.GetBytes(otp)));
```

The `Otp:HashKey` secret must be configured in User Secrets. The original OTP is sent to the user; the hash is stored in the database.

### 6. Save the Verification Record

Create a record linked to the newly registered user:

```csharp
var verification = new EmailVerificationModel
{
    UserId = user.Id,
    OtpHash = otpHash,
    ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
    CreatedAtUtc = DateTime.UtcNow,
    IsUsed = false,
    Attempts = 0
};

_context.EmailVerifications.Add(verification);
await _context.SaveChangesAsync();
```

The record stores the OTP hash, expiration time, creation time, and usage status. The code expires after ten minutes.

### 7. Send the OTP Email

After saving the verification record, send the original `otp` to the registered email address using the Gmail SMTP and MailKit implementation described in the **Email OTP Setup** section.

Return a signup response only after the email-sending operation succeeds.

### 8. Required Namespaces

Ensure `AuthService.cs` includes the following namespaces:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using MunPath.Models.EmailVerification;
```

Include your existing namespaces for the DTOs, user models, mapper, and database context.

### Registration Flow Summary

1. Validate that the email is not already registered.
2. Retrieve the Student role.
3. Create the user and hash their password.
4. Save the user to the database.
5. Generate and hash a six-digit OTP.
6. Save the verification record with a ten-minute expiration.
7. Send the OTP email.
8. Return the signup response.

**Security note:** Keep the OTP hash key and Gmail App Password private. Also validate the OTP's expiration, usage status, and attempt count when implementing `VerifyEmailAsync()`.


=================================================================================================================================


# MunPath Email OTP Setup (Gmail + MailKit)

This guide explains how to send a six-digit email verification code during user registration using Gmail SMTP, MailKit, and MimeKit.

## 1. Install NuGet Packages

Open the terminal in the folder containing `MunPath.csproj` and run:

```bash
dotnet add package MailKit
dotnet add package MimeKit
```

## 2. Generate a Gmail App Password

1. Sign in to your Google Account.
2. Enable **2-Step Verification**.
3. Open [Google App Passwords](https://myaccount.google.com/apppasswords).
4. Generate an app password for MunPath.
5. Copy the generated password.

Use the App Password instead of your normal Gmail password.

## 3. Configure User Secrets

Initialize User Secrets if you haven't already:

```bash
dotnet user-secrets init
```

Store your Gmail credentials:

```bash
dotnet user-secrets set "EmailSettings:Email" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:AppPassword" "your-app-password"
```

Replace the example values with your actual credentials. Never commit these secrets to Git.

## 4. Configure the Email Service

In `AuthService.cs`, inject `IConfiguration` through the constructor so the service can access User Secrets:

```csharp
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
```

Keep any other dependencies already used by your service.

Add these namespaces:

```csharp
using Microsoft.Extensions.Configuration;
using MailKit.Security;
using MimeKit;
```

## 5. Generate and Store the OTP

Inside `RegisterAsync()`, generate a six-digit code:

```csharp
string otp = RandomNumberGenerator
    .GetInt32(0, 1_000_000)
    .ToString("D6");
```

Hash the OTP and save the hash, user ID, expiration time, and usage status in your `EmailVerifications` table.

Set the expiration time to ten minutes:

```csharp
ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
```

Store the hash, not the original OTP, in the database.

## 6. Send the Verification Email

After saving the verification record, use MailKit and MimeKit to send the original OTP:

```csharp
var email = _configuration["EmailSettings:Email"]
    ?? throw new InvalidOperationException(
        "EmailSettings:Email is missing from configuration.");

var password = _configuration["EmailSettings:AppPassword"]
    ?? throw new InvalidOperationException(
        "EmailSettings:AppPassword is missing from configuration.");

var message = new MimeMessage();

message.From.Add(new MailboxAddress("MunPath", email));
message.To.Add(MailboxAddress.Parse(request.Email));
message.Subject = "Your MunPath verification code";

message.Body = new TextPart("plain")
{
    Text = $"Your verification code is {otp}. " +
           "It expires in 10 minutes."
};

using var smtp = new MailKit.Net.Smtp.SmtpClient();

await smtp.ConnectAsync(
    "smtp.gmail.com",
    587,
    SecureSocketOptions.StartTls);

await smtp.AuthenticateAsync(email, password);
await smtp.SendAsync(message);
await smtp.DisconnectAsync(true);
```

Add this namespace for OTP generation:

```csharp
using System.Security.Cryptography;
```

Place the email-sending code inside `RegisterAsync()`, after generating the OTP and saving its verification record. Return your `SignUpResponseDto` after sending the email successfully.

## 7. Test the Signup Flow

Run the application:

```bash
dotnet watch run
```

1. Open the signup page.
2. Register using a valid email address.
3. Check the recipient's inbox and spam folder.
4. Confirm that the verification record exists in the database.
5. Verify that the account remains unverified until the OTP is successfully confirmed.

## 8. Important Security Notes

- Never store Gmail credentials in source code.
- Never commit User Secrets or App Passwords to Git.
- Expire OTPs after ten minutes.
- Limit incorrect OTP attempts.
- Mark successful OTPs as used to prevent reuse.
- Handle SMTP failures so users can request another verification code.

**SMTP settings**

| Setting | Value |
|---|---|
| SMTP host | `smtp.gmail.com` |
| Port | `587` |
| Security | STARTTLS |
| Authentication | Gmail address + App Password |