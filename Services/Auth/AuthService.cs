using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MunPath.Data;
using MunPath.DTOs.Auth;
using MunPath.Models.User;

namespace MunPath.Services.Auth
{
  public class AuthService : IAuthService
  {
    private readonly ApplicationDbContext _context;

    private readonly PasswordHasher<UserModel> _passwordHasher;

    public AuthService(ApplicationDbContext context, PasswordHasher<UserModel> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }


    public Task<bool> LoginAsync(LoginRequestDto requestDto)
    {
      throw new NotImplementedException();
    }

    public Task<SignUpResponseDto> RegisterAsync(SignUpRequestDto request)
    {
      throw new NotImplementedException();
    }

    public Task<bool> VerifyEmailAsync(VerifyEmailRequestDto request)
    {
      throw new NotImplementedException();
    }
  }
}