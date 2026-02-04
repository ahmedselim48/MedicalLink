using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using MedLink.Application.Common.JWT;
using MedLink.Application.DTOs.Identity;
using MedLink.Application.Interfaces.Services;
using MedLink.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MedLink.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly Jwt _jwt;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IProfileService _profileService;

        public AuthService(
            UserManager<ApplicationUser> userManager, 
            IOptions<Jwt> jwt, 
            IMapper mapper, 
            RoleManager<IdentityRole> roleManager, 
            IEmailService emailService, 
            ISmsService smsService, 
            IProfileService profileService)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _mapper = mapper;
            _roleManager = roleManager;
            _emailService = emailService;
            _smsService = smsService;
            _profileService = profileService;
        }

        // =================================================================================================
        // 1. Authentication & Registration
        // =================================================================================================

        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            var isEmail = new EmailAddressAttribute().IsValid(model.Email);
            var existingUser = await _userManager.FindByEmailAsync(model.Email);

            if (existingUser is not null)
                return new AuthModel { Message = "Email already registered" };

            var user = _mapper.Map<ApplicationUser>(model);

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthModel { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "User");
            try
            {
                await _profileService.CreateAsync(user.Id, model.FullName);
            }
            catch (Exception ex)
            {
                await _userManager.DeleteAsync(user);
                return new AuthModel { Message = "Failed to create user profile: " + ex.Message };
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{_jwt.Issuer}/api/auth/confirm-email?userId={user.Id}&code={code}";

            var message = new EmailMessage([user.Email], "Confirm Your Email", callbackUrl);
            await _emailService.SendEmailAsync(message);

            return new AuthModel
            {
                Message = "User registered successfully. Please verify your email.",
                IsAuthenticated = false,
                Email = user.Email,
                Username = model.Email.Split('@')[0]
            };
        }

        public async Task<AuthModel> GetTokenAsync(RequestTokenModel model)
        {
            var authModel = new AuthModel();
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authModel.Message = "Email or Password is incorrect!";
                return authModel;
            }

            if (!user.EmailConfirmed)
            {
                authModel.Message = "Email is not confirmed";
                return authModel;
            }

            if (user.IsDeleted)
            {
                authModel.Message = "Account is deleted";
                return authModel;
            }

            var token = await CreateJwtToken(user);
            authModel.Message = "Token created successfully";
            authModel.IsAuthenticated = true;
            authModel.Email = user.Email;
            authModel.Username = user.UserName;
            authModel.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            authModel.ExpiresOn = token.ValidTo;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(token);

            return authModel;
        }

        public async Task<AuthModel> LoginWithGoogleAsync(string email, string name, string googleId)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null && user.IsDeleted)
                return new AuthModel { Message = "Account is deleted" };

            if (user == null)
            {
                var baseUserName = email.Split('@')[0];
                var userName = baseUserName;
                while (await _userManager.FindByNameAsync(userName) != null)
                {
                    userName = $"{baseUserName}{Random.Shared.Next(1000, 9999)}";
                }

                // Create user if not exists
                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    FullName = name,
                    EmailConfirmed = true // Trusted source
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new AuthModel { Message = errors };
                }

                await _userManager.AddToRoleAsync(user, "User");
                await _profileService.CreateAsync(user.Id, user.FullName);
                await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", googleId, "Google"));
            }
            else
            {
                // Ensure login is linked
                var logins = await _userManager.GetLoginsAsync(user);
                if (!logins.Any(l => l.LoginProvider == "Google" && l.ProviderKey == googleId))
                {
                    await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", googleId, "Google"));
                }
            }

            var token = await CreateJwtToken(user);

            return new AuthModel
            {
                Message = "Login successful",
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = user.Email,
                Username = user.UserName,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                ExpiresOn = token.ValidTo
            };
        }

        // =================================================================================================
        // 2. Account Management
        // =================================================================================================

        public async Task<string> ChangePasswordAsync(ChangePasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return "User not found";

            if (user.IsDeleted) return "Account is deleted";

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return "Password changed successfully";
        }

        public async Task<string> DeleteAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return "User not found";

            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded ? "Account deleted successfully" : "Error deleting account";
        }

        public async Task<AuthModel> RestoreAccountAsync(RequestTokenModel model)
        {
            var authModel = new AuthModel();
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authModel.Message = "Email or Password is incorrect!";
                return authModel;
            }

            if (!user.IsDeleted)
            {
                authModel.Message = "Account is not deleted";
                return authModel;
            }

            user.IsDeleted = false;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                authModel.Message = "Error restoring account";
                return authModel;
            }

            var token = await CreateJwtToken(user);
            authModel.Message = "Account restored and logged in successfully";
            authModel.IsAuthenticated = true;
            authModel.Email = user.Email;
            authModel.Username = user.UserName;
            authModel.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            authModel.ExpiresOn = token.ValidTo;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(token);

            return authModel;
        }

        // =================================================================================================
        // 3. Password Recovery
        // =================================================================================================

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email!);

            if (user is null)
                return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var param = new Dictionary<string, string?>
            {
                {"token",token},
                {"email",model.Email }
            };

            var callback = QueryHelpers.AddQueryString(model.ClientUri, param);

            var message = new EmailMessage([user.Email], "Reset Your Password", callback);
            await _emailService.SendEmailAsync(message);

            return true;
        }

        public async Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email!);

            if (user is null)
                return new ResetPasswordResult(false, "user not found Request");

            var result = await _userManager.ResetPasswordAsync(user, model.Token!, model.Password!);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new ResetPasswordResult(false, errors);
            }

            return new ResetPasswordResult(true, "Password Changed Successfully");
        }

        // =================================================================================================
        // 4. Verification (Email & Phone)
        // =================================================================================================

        public async Task<string> ConfirmEmailAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return "User not found";

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            return result.Succeeded ? "Email confirmed successfully" : "Error confirming email";
        }

        public async Task<string> SendPhoneVerificationAsync(string email, string phoneNumber)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return "User not found";

            if (!phoneNumber.StartsWith("+"))
                phoneNumber = "+" + phoneNumber;

            try
            {
                var status = await _smsService.SendVerificationTokenAsync(phoneNumber);
                return $"Verification code sent to {phoneNumber}. Status: {status}";
            }
            catch (Exception ex)
            {
                return $"Failed to send verification code: {ex.Message}";
            }
        }

        public async Task<string> ConfirmPhoneNumberAsync(string email, string code, string phoneNumber)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return "User not found";

            if (!phoneNumber.StartsWith("+"))
                phoneNumber = "+" + phoneNumber;

            try
            {
                var isValid = await _smsService.VerifyTokenAsync(phoneNumber, code);
                if (!isValid) return "Invalid code";

                user.PhoneNumber = phoneNumber;
                user.PhoneNumberConfirmed = true;
                await _userManager.UpdateAsync(user);

                return "Phone number confirmed successfully";
            }
            catch (Exception ex)
            {
                return $"Verification failed: {ex.Message}";
            }
        }

        // =================================================================================================
        // 5. Administration
        // =================================================================================================

        public async Task<string> AddRoleAsync(AddRoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user is null || !await _roleManager.RoleExistsAsync(model.Role))
                return "Invalid User ID or Role";

            if (await _userManager.IsInRoleAsync(user, model.Role))
                return "User already assigned to this role";

            var result = await _userManager.AddToRoleAsync(user, model.Role);

            return result.Succeeded ? string.Empty : "Something went wrong";
        }

        // =================================================================================================
        // 6. Private Helpers
        // =================================================================================================

        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
            {
                roleClaims.Add(new Claim("roles", role));
            }

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim("uid",user.Id)
            };

            if (!string.IsNullOrWhiteSpace(user.Email))
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));

            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                claims.Add(new Claim("phone_number", user.PhoneNumber));

            claims.AddRange(userClaims);
            claims.AddRange(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(Convert.ToDouble(_jwt.DurationInDays)),
                signingCredentials: signingCredentials
            );

            return jwtSecurityToken;
        }
    }
}