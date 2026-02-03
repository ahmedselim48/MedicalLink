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

        //Log Exception                        //Check Environment
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }
        public async Task InvokeAsync(HttpContext context) //When CLR execute this middleware, the method will be implemented
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                int statusCode = (int)HttpStatusCode.InternalServerError;

                switch (ex)
                {
                    case NotFoundException:
                    case KeyNotFoundException:
                        statusCode = (int)HttpStatusCode.NotFound;
                        break;
                    case BadRequestException:
                    case InvalidOperationException:
                        statusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    case UnauthorizedAccessException:
                        statusCode = (int)HttpStatusCode.Forbidden;
                        break;
                    default:
                        statusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                if (statusCode == (int)HttpStatusCode.InternalServerError)
                {
                    _logger.LogError(ex, ex.Message);
                }
                else
                {
                    _logger.LogWarning("Client Error ({StatusCode}): {Message}", statusCode, ex.Message);
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
