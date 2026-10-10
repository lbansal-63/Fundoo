using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ModelLayer.DTOs;
using ModelLayer.Entities;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Services
{
    public class UserBusiness : IUserBusiness
    {
        private readonly IUserRepository _repo;
        private readonly IConfiguration _config;
        private readonly EmailHelper _email;

        public UserBusiness(IUserRepository repo, IConfiguration config, EmailHelper email)
        {
            _repo = repo;
            _config = config;
            _email = email;
        }

        private SymmetricSecurityKey GetKey()
        {
            var keyConfig = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key missing in appsettings.json");
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyConfig));
        }


        public bool Register(RegisterDto dto)
        {
            if (_repo.GetByEmail(dto.Email) != null)
                return false;

            _repo.Add(new UserEntity
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            });
            return true;
        }

        public string? Login(LoginDto dto)
        {
            var user = _repo.GetByEmail(dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            var token = new JwtSecurityToken(
                claims: new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                },
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(GetKey(), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        public void ForgotPassword(string email)
        {
            var user = _repo.GetByEmail(email);

            // Do not reveal whether an email is registered.
            if (user == null)
                return;

            var resetToken = new JwtSecurityToken(
                claims: new[]
                {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("purpose", "reset")
                },
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: new SigningCredentials(
                    GetKey(),
                    SecurityAlgorithms.HmacSha256)
            );

            var token = new JwtSecurityTokenHandler()
                .WriteToken(resetToken);

            var subject = "Fundoo - Password Reset Token";
            var body = $"""
        Hello,

        Use the token below to reset your Fundoo password.
        This token expires in 15 minutes.

        Reset token:
        {token}

        If you did not request this, ignore this email.
        """;

            _email.Send(user.Email, subject, body);
        }

        public bool ResetPassword(ResetPasswordDto dto)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();

                var principal = handler.ValidateToken(dto.Token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = GetKey()
                }, out _);

                
                if (principal.FindFirst("purpose")?.Value != "reset")
                    return false;

                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                if (email == null) return false;

                var user = _repo.GetByEmail(email);
                if (user == null) return false;

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                _repo.Update(user);
                return true;
            }
            catch
            {
                return false; 
            }
        }
    }
}