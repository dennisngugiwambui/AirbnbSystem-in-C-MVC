using BevoBnB.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BevoBnB.ViewModels
{
    public class MessageViewModel
    {
        public int MessageID { get; set; }
        public string SenderName { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTime SentDate { get; set; }
        public MessageStatus Status { get; set; }
        public string PropertyNumber { get; set; }
        public List<MessageReplyViewModel> Replies { get; set; } = new List<MessageReplyViewModel>();
    }

    public class MessageReplyViewModel
    {
        public string SenderName { get; set; }
        public string Content { get; set; }
        public DateTime SentDate { get; set; }
    }

    public class CreateMessageViewModel
    {
        [Required]
        [StringLength(100)]
        public string Subject { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; }

        [Required]
        public string ReceiverID { get; set; }

        public int? PropertyID { get; set; }
    }

    public class MessageReplyCreateViewModel
    {
        [Required]
        public int MessageID { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; }
    }
}
