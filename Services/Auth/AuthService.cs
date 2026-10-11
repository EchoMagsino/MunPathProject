using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MunPath.Data;
using MunPath.DTOs.Auth;
using MunPath.Mappers.SignUp;
using MunPath.Models.User;
using System.Security.Cryptography;
using System.Text;
using MunPath.Models.EmailVerification;
using MimeKit;
using MailKit.Security;


namespace MunPath.Services.Auth
{
  public class AuthService : IAuthService
  {
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<UserModel> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, PasswordHasher<UserModel> passwordHasher, IConfiguration configuration)
    {
      _context = context;
      _passwordHasher = passwordHasher;
      _configuration = configuration;
    }


    public Task<bool> LoginAsync(LoginRequestDto requestDto)
    {
      throw new NotImplementedException();
    }

    public async Task<SignUpResponseDto> RegisterAsync(SignUpRequestDto request)
    {

      bool emailExist = await _context.Users
        .AnyAsync(u => u.Email == request.Email);

      if (emailExist)
      {
        throw new InvalidOperationException("This email is already registered");
      }

      var userRole = await _context.Roles
        .FirstOrDefaultAsync(r => r.RoleName == "Student");

      if (userRole == null)
      {
        throw new InvalidOperationException("The default user role was not found.");
      }

      var user = SignUpMapper.ToUserModel(request);

      user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

      user.RoleName = userRole.RoleName;
      user.Role = userRole;

      _context.Users.Add(user);
      await _context.SaveChangesAsync();

      string otp = RandomNumberGenerator
         .GetInt32(0, 1_000_000)
         .ToString("D6");

      var secretKey = _configuration["Otp:HashKey"]
    ?? throw new InvalidOperationException(
        "Otp:HashKey is missing from configuration.");

      string otpHash = Convert.ToHexString(
          HMACSHA256.HashData(
              Encoding.UTF8.GetBytes(secretKey),
              Encoding.UTF8.GetBytes(otp)));


      // 4. Save the verification record HERE
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

      //Email sender


      var email = _configuration["EmailSettings:Email"]
          ?? throw new InvalidOperationException(
              "EmailSettings:Email is missing from configuration.");

      var password = _configuration["EmailSettings:AppPassword"]
          ?? throw new InvalidOperationException(
              "EmailSettings:AppPassword is missing from configuration.");

      var message = new MimeMessage();

      message.From.Add(
          new MailboxAddress("MunPath", email));

      message.To.Add(
          MailboxAddress.Parse(request.Email));

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

      // Return the response
      return new SignUpResponseDto
      {
        Message = "Registration successful. Please check your email for the verification code."
      };

    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailRequestDto request)
    {

      try {

       if(string.IsNullOrWhiteSpace(request.Otp) || request.Otp.Length != 6 || !request.Otp.All(char.IsDigit))
      {
          return false;
      }

      var user = await _context.Users
          .FirstOrDefaultAsync(u => u.Email == request.Email);

      if(user == null || user.EmailVerified)
      {
        return false;
      }

      //find the latest unused otp
      var verification = await  _context.EmailVerifications
          .Where(v => v.UserId == user.Id && !v.IsUsed)
          .OrderByDescending(v => v.CreatedAtUtc)
          .FirstOrDefaultAsync();

    
        if(verification == null)
      {
          return false;
      }

        if (verification.ExpiresAtUtc <= DateTime.UtcNow)
      {
          verification.IsUsed = true;
          await _context.SaveChangesAsync();
          return false;
      }

      if(verification.Attempts >= 5)
      {
        verification.IsUsed = true;
        await _context.SaveChangesAsync();
        return false;
      }

          // 6. Generate the submitted OTP's hash
    var hashKey = _configuration["Otp:HashKey"]
        ?? throw new InvalidOperationException(
            "Otp:HashKey is missing from configuration.");

    string submittedHash = Convert.ToHexString(
        HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(hashKey),
            Encoding.UTF8.GetBytes(request.Otp)));

    // 7. Compare hashes safely
    bool isValid = CryptographicOperations.FixedTimeEquals(
        Convert.FromHexString(verification.OtpHash),
        Convert.FromHexString(submittedHash));

      if(!isValid)
      {
        verification.Attempts++;

        if(verification.Attempts >= 5)
        {
          verification.IsUsed = true;
        }

        await _context.SaveChangesAsync();
        return false;

      }
      

      // 8. Mark the OTP as used and verify the user's email

    verification.IsUsed = true;
  
    user.EmailVerified = true;
  


    await _context.SaveChangesAsync();
    return true;

      }
      catch (Exception ex)
      {
          Console.WriteLine($"VerifyEmailAsync failed: {ex}");
          throw;
      }
       
    }
  }
}