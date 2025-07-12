namespace TimesynqServer.Models.DTO
{
    public class ResponseDTO<T>
    {
        public required int StatusCode { get; set; }
        public ICollection<string>? Errors { get; set; }
        public T? Result { get; set; }
    }
}
