namespace Core.Models.BroadcastModel
{
    public class BroadcastRecipientDto
    {
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsRead { get; set; }
    }
}
