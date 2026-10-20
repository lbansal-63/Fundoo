using ModelLayer;

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