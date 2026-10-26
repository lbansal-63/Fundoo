namespace Middleware.ExceptionHandling
{
    // 409 Conflict: jaise duplicate label naam
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
}