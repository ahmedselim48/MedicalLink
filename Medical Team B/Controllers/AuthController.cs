using MedLink.Application.DTOs.Identity;
using MedLink.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medical_Team_B.Controllers
{

    /// <summary>
    /// Manages authentication and user identity operations.
    /// </summary>
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="registerDto">The registration details.</param>
        [HttpPost("register")]
        public async Task<ActionResult<AuthModel>> RegisterAsync([FromBody] RegisterModel registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(registerDto);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }

        /// <summary>
        /// Logs in a user and returns a JWT token.
        /// </summary>
        /// <param name="model">The login credentials.</param>
        [HttpPost("login")]
        public async Task<ActionResult<AuthModel>> GetTokenAsync([FromBody] RequestTokenModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.GetTokenAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }

        /// <summary>
        /// Initiates Google authentication.
        /// </summary>
        [HttpGet("signin-google")]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth");
            var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Handles Google authentication callback.
        /// </summary>
        [HttpGet("google-response")]
        public async Task<ActionResult<AuthModel>> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
                return BadRequest("Google authentication failed");

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;
            var googleId = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (email == null || googleId == null)
                return BadRequest("Could not retrieve email or Google ID");

            var authModel = await _authService.LoginWithGoogleAsync(email, name, googleId);
            return Ok(authModel);
        }

        /// <summary>
        /// Changes the user's password.
        /// </summary>
        /// <param name="model">The change password details.</param>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ChangePasswordAsync(model);

            if (!result.Equals("Password changed successfully", StringComparison.OrdinalIgnoreCase))
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Deletes the user's account.
        /// </summary>
        [Authorize]
        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
            if (userId == null) return Unauthorized();

            var result = await _authService.DeleteAccountAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Restores a deleted account.
        /// </summary>
        /// <param name="model">The credentials to verify account ownership.</param>
        [HttpPost("restore-account")]
        public async Task<ActionResult<AuthModel>> RestoreAccountAsync([FromBody] RequestTokenModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RestoreAccountAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }

        /// <summary>
        /// Initiates the forgot password process.
        /// </summary>
        /// <param name="model">The email of the user.</param>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var isVerificationEmailSent = await _authService.ForgotPasswordAsync(model);

            if (!isVerificationEmailSent)
                return BadRequest("Invalid Request");
            return Ok();
        }

        /// <summary>
        /// Resets the user's password.
        /// </summary>
        /// <param name="model">The reset password details.</param>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var result = await _authService.ResetPasswordAsync(model);

            if (!result.Success)
                return BadRequest(new { Errors = result.Message });
            return Ok(result.Message);
        }

        /// <summary>
        /// Confirms the user's email address.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="code">The confirmation code.</param>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
                return BadRequest("User ID and Code are required");

            var result = await _authService.ConfirmEmailAsync(userId, code);
            return Ok(result);
        }

        /// <summary>
        /// Sends a verification code to the user's phone.
        /// </summary>
        /// <param name="model">The phone verification details.</param>
        [HttpPost("verify-phone/send")]
        public async Task<IActionResult> SendPhoneVerification([FromBody] SendPhoneVerificationModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.SendPhoneVerificationAsync(model.Email, model.PhoneNumber);
            return Ok(result);
        }

        /// <summary>
        /// Verifies the code sent to the user's phone.
        /// </summary>
        /// <param name="model">The phone confirmation details.</param>
        [HttpPost("verify-phone/confirm")]
        public async Task<IActionResult> ConfirmPhone([FromBody] ConfirmPhoneModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ConfirmPhoneNumberAsync(model.Email, model.Code, model.PhoneNumber);
            return Ok(result);
        }

        /// <summary>
        /// Adds a role to a user (Admin only).
        /// </summary>
        /// <param name="model">The role addition details.</param>
        [Authorize(Roles = "Admin")]
        [HttpPost("addrole")]
        public async Task<ActionResult<string>> AddRoleAsync([FromBody] AddRoleModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.AddRoleAsync(model);

            return !string.IsNullOrEmpty(result) ? BadRequest(result) : Ok(model);
        }
    }
}