namespace MedLink.Application.DTOs.Identity
{
    public class ResetPasswordResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; } = string.Empty;

        public ResetPasswordResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }


    }
}
