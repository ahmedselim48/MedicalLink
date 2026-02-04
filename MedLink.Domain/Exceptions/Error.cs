using Microsoft.AspNetCore.Http;

namespace Domain.ErrorHandling
{
    public record Error(string Code, string Description, int? StatusCode)
    {
        public static readonly Error None = new(string.Empty, string.Empty, null);

      
        public static Error Unauthorized(string description)
            => new("Unauthorized", description, StatusCodes.Status401Unauthorized);

        public static Error Forbidden(string description)
            => new("Forbidden", description, StatusCodes.Status403Forbidden);

        public static Error Validation(string description)
            => new("Validation", description, StatusCodes.Status400BadRequest);

        public static Error NotFound(string description)
            => new("NotFound", description, StatusCodes.Status404NotFound);

        public static Error Conflict(string description)
            => new("Conflict", description, StatusCodes.Status409Conflict);

        public static Error InternalServer(string description)
            => new("InternalServer", description, StatusCodes.Status500InternalServerError);
    }
}
