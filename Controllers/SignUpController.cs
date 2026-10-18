using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MunPath.DTOs.Auth;
using MunPath.Services.Auth;

namespace MunPath.Controllers
{
    [Route("[controller]")]
    public class SignUpController : Controller
    {
        private readonly ILogger<SignUpController> _logger; 
        private readonly AuthService _authService;


        public SignUpController(ILogger<SignUpController> logger, AuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SignUpRequestDto signup)
        {
            if(!ModelState.IsValid)
            {
                return View(signup);
            }

            try
            {
               await _authService.RegisterAsync(signup);

               TempData["SuccessMessage"] = "Account created! Please verify your email";

               return RedirectToAction("Index", "VerifyEmail", new { email = signup.Email });

            }
            catch(InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(signup);

            }
            
        }

   
       



        
    }
}
