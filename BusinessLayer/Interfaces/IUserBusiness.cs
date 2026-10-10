using ModelLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IUserBusiness
    {
        bool Register(RegisterDto dto);
        string? Login(LoginDto dto);
        void ForgotPassword(string email);
        bool ResetPassword(ResetPasswordDto dto);
    }
}
