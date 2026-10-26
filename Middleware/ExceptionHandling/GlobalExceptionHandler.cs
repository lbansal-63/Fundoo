using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Middleware.ExceptionHandling
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetails;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IProblemDetailsService problemDetails)
        {
            _logger = logger;
            _problemDetails = problemDetails;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (status, title) = exception switch
            {
                RequestValidationException => (StatusCodes.Status400BadRequest, "Bad request"),
                NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                BadHttpRequestException bre => (bre.StatusCode, "Bad request"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                _ => (StatusCodes.Status500InternalServerError, "Server error")
            };

            if (status >= 500)
                _logger.LogError(exception, "Unhandled exception on {Method} {Path}",
                    httpContext.Request.Method, httpContext.Request.Path);
            else
                _logger.LogWarning(exception, "Request failed on {Method} {Path}",
                    httpContext.Request.Method, httpContext.Request.Path);

            httpContext.Response.StatusCode = status;

            // Hamari apni exceptions ke messages safe hain (hum khud likhte hain), client ko dikhate hain.
            // 500 me exception ka message/stack trace client ko kabhi nahi jaata.
            var isOwnException = exception is NotFoundException
                or ConflictException
                or RequestValidationException;

            return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = isOwnException
                        ? exception.Message
                        : status >= 500 ? "Something went wrong, Try again later!!" : null
                }
            });
        }
    }
}