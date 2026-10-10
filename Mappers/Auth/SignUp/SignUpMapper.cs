using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MunPath.DTOs.Auth;
using MunPath.Models.User;

namespace MunPath.Mappers.SignUp
{
    public class SignUpMapper
    {
        public static UserModel ToUserModel(SignUpRequestDto signup)
        {
            return new UserModel
            {
                FirstName = signup.FirstName,
                LastName = signup.LastName,
                Email = signup.Email,
                EmailVerified = false
            };
        }

        public static SignUpResponseDto ToSignUpResponseDto(UserModel user)
        {
            return new SignUpResponseDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RequiresEmailVerification = !user.EmailVerified
            };
        }
    }
}