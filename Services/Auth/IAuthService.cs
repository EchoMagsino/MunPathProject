using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MunPath.DTOs.Auth;

namespace MunPath.Services.Auth
{
    public interface IAuthService
    {
        Task<SignUpResponseDto> RegisterAsync(SignUpRequestDto request);

        Task<bool> VerifyEmailAsync(VerifyEmailRequestDto request);

        Task<bool> LoginAsync(LoginRequestDto requestDto);

        
    }
}