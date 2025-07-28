namespace TimesynqServer.Models
{
    public class DomainError(int code, string message)
    {
        public int Code => code;
        public string Message => message;
    }

    public static class DomainErrors
    {
        
    }
}
