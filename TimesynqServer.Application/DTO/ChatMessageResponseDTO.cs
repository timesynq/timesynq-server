namespace TimesynqServer.Application.DTO
{
    public class ChatMessageResponseDTO
    {
        public Guid UserId { get; }
        public string Message { get; }

        private ChatMessageResponseDTO(Guid userId, string message)
        {
            UserId = userId;
            Message = message;
        }

        public static ChatMessageResponseDTO FromChatMessage(ChatMessageDTO chatMessageDTO)
        {
            return new(chatMessageDTO.UserId, chatMessageDTO.Message);
        }
    }
}