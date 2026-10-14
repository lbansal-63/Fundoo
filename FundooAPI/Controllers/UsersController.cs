using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.DTOs;
using Microsoft.AspNetCore.RateLimiting;

namespace FundooAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("auth")]
    public class UsersController : ControllerBase
    {
        private readonly IUserBusiness _business;

        public UsersController(IUserBusiness business)
        {
            _business = business;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterDto dto)
        {
            if (_business.Register(dto))
                return Ok("Registered successfully");
            return Conflict("Email already registered");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            var token = _business.Login(dto);
            if (token == null)
                return Unauthorized("Invalid email or password");
            return Ok(new { token });
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswordDto dto)
        {
            _business.ForgotPassword(dto.Email);
            return Ok("Agar email registered hai to mail bhej di gayi hai");
        }

        
        [HttpPost("reset-password")]
        public IActionResult ResetPassword(ResetPasswordDto dto)
        {
            if (_business.ResetPassword(dto))
                return Ok("Password reset successful");
            return BadRequest("Invalid or expired reset token");
        }
    }
}