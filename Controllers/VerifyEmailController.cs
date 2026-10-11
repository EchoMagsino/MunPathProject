using Microsoft.AspNetCore.Mvc;
using MunPath.DTOs.Auth;
using MunPath.Services.Auth;

namespace MunPath.Controllers
{
    [Route("[controller]")]
    public class VerifyEmailController : Controller
    {
        private readonly AuthService _authService;

        public VerifyEmailController(AuthService authService)
        {
            _authService = authService;
        }

        // GET: /VerifyEmail
        [HttpGet("")]
        public IActionResult Index(string email)
        {
            var model = new VerifyEmailRequestDto
            {
                Email = email
            };

            return View("~/Views/VerifyEmail/Index.cshtml", model);
        }

        // POST: /VerifyEmail
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(
            VerifyEmailRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/VerifyEmail/Index.cshtml", request);
            }

            bool verified = await _authService.VerifyEmailAsync(request);

            if (!verified)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid or expired verification code.");

                return View("~/Views/VerifyEmail/Index.cshtml", request);
            }

            TempData["SuccessMessage"] =
                "Your email has been verified successfully!";

            return RedirectToAction("Index", "Login");
        }
    }
}