using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MunPath.DTOs.Auth;
using MunPath.Models.User;

namespace MunPath.Mappers.User
{
    public class UserMapper
    {
        public static UserResponseDto ToUserResponseDto(UserModel user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }
    }
}