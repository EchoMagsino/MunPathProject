using Microsoft.EntityFrameworkCore;
using MunPath.Services.Auth;
using MunPath.Data;
using Microsoft.AspNetCore.Identity;
using MunPath.Models.User;



namespace MunPath.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ApplicationService(this IServiceCollection services, IConfiguration configuration)
        {
             services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

             services.AddScoped<AuthService>();
             services.AddScoped<PasswordHasher<UserModel>>();
             

            return services;
        }
    }
}