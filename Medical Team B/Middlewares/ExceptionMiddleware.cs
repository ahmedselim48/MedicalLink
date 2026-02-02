using System.Net;
using System.Text.Json;
using Medical_Team_B.Errors;
using MedLink.Domain.Exceptions;

namespace Medical_Team_B.Middlewares
{

    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }
        public async Task InvokeAsync(HttpContext context) 
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                
                context.Response.ContentType = "application/json";
                
                int statusCode = (int)HttpStatusCode.InternalServerError;
                var result = string.Empty;

                switch (ex)
                {
                    case NotFoundException notFoundException:
                        statusCode = (int)HttpStatusCode.NotFound;
                        break;
                    // You can add more custom exceptions here later (e.g., ValidationException -> 400)
                    default:
                        statusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                context.Response.StatusCode = statusCode;

                var response = _env.IsDevelopment() 
                    ? new ApiExceptionResponse(statusCode, ex.Message, ex.StackTrace?.ToString())
                    : new ApiExceptionResponse(statusCode, ex.Message); // For NotFound, we usually want the message even in Prod

                if (statusCode == (int)HttpStatusCode.InternalServerError && !_env.IsDevelopment())
                {
                    response = new ApiExceptionResponse(statusCode); // Generic message for real server errors in Prod
                }

                var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);

            }
        }





    }
}
