using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MunPath.DTOs.Auth
{
    public class VerifyEmailRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{6}$",
            ErrorMessage = "Enter a valid six-digit OTP.")]
        public string Otp { get; set; } = string.Empty;

        
    }
}