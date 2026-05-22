namespace Core.Models.MessageModel
{
    public class DialogResponse
    {
        public long WithUserId { get; set; }
        public List<MessageResponse> Messages { get; set; } = [];
    }
}
