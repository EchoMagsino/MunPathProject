using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MunPath.DTOs.Auth
{
    public class SignUpResponseDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public bool RequiresEmailVerification { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}