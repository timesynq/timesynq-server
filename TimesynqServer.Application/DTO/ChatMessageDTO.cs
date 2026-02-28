namespace TimesynqServer.Application.DTO
{
    public class ChatMessageDTO
    {
        public Guid WipId { get; }
        public Guid UserId { get; }
        public string Message { get; }

        public ChatMessageDTO(Guid wipId, Guid userId, string message) 
        {
            WipId = wipId;
            UserId = userId;
            Message = message;
        }
    }
}