using MedLink.Application.DTOs.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterModel registerModel);
        Task<AuthModel> GetTokenAsync(RequestTokenModel model);
        Task<string> AddRoleAsync(AddRoleModel model);
        Task<bool> ForgotPasswordAsync(ForgotPasswordModel model);
        Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordModel model);
        Task<string> SendPhoneVerificationAsync(string email, string phoneNumber);
        Task<string> ConfirmPhoneNumberAsync(string email, string code, string phoneNumber);
        Task<AuthModel> LoginWithGoogleAsync(string email, string name, string googleId);
        Task<string> ConfirmEmailAsync(string userId, string code);
        Task<string> DeleteAccountAsync(string userId);
        Task<AuthModel> RestoreAccountAsync(RequestTokenModel model);
        Task<string> ChangePasswordAsync(ChangePasswordModel model);


    }
}
