namespace Middleware.ExceptionHandling
{
    // 400 Bad Request: validator ne input galat paya
    public class RequestValidationException : Exception
    {
        public RequestValidationException(string message) : base(message)
        {
        }
    }
}