using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ErrorHandling
{
    public static class EntityError<T>
    {
        public static Error NotFound(string? customMessage = null) =>
            new($"{typeof(T).Name}.NotFound",
                customMessage ?? $"{typeof(T).Name} not found",
                StatusCodes.Status404NotFound);

        public static Error Duplicated(string? customMessage = null) =>
            new($"{typeof(T).Name}.Duplicated",
                customMessage ?? $"Another {typeof(T).Name} already exists",
                StatusCodes.Status409Conflict);

        public static Error InvalidData(string? customMessage) =>
            new($"{typeof(T).Name}.InvalidData",
                customMessage ?? $"Invalid {typeof(T).Name} data",
                StatusCodes.Status400BadRequest);

        public static Error OperationFailed(string? customMessage = null) =>
            new($"{typeof(T).Name}.OperationFailed",
                customMessage ?? $"Operation on {typeof(T).Name} failed",
                StatusCodes.Status500InternalServerError);
    }
}
