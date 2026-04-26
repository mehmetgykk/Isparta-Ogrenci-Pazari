using System;

namespace UserAuthApi.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string ReceiverUsername { get; set; } = string.Empty;
        public string AdTitle { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; } = false;
        public int? ParentMessageId { get; set; }
    }
}