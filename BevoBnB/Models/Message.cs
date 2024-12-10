using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BevoBnB.Models
{
    public enum MessageStatus
    {
        Unread,
        Read,
        Archived
    }
        public class Message
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int MessageID { get; set; }

            [Required]
            [ForeignKey("Sender")]
            public string SenderID { get; set; }

            [Required]
            [ForeignKey("Receiver")]
            public string ReceiverID { get; set; }

            [Required]
            [StringLength(100)]
            public string Subject { get; set; }

            [Required]
            [StringLength(2000)]
            public string Content { get; set; }

            [Required]
            public DateTime SentDate { get; set; } = DateTime.Now;

            [Required]
            public MessageStatus Status { get; set; } = MessageStatus.Unread;

            [ForeignKey("Property")]
            public int? PropertyID { get; set; }

            public virtual Property Property { get; set; }
            public virtual Users Sender { get; set; }
            public virtual Users Receiver { get; set; }
            public virtual ICollection<MessageReply> Replies { get; set; } = new List<MessageReply>();
        }

        public class MessageReply
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int ReplyID { get; set; }

            [Required]
            [ForeignKey("Message")]
            public int MessageID { get; set; }

            [Required]
            [ForeignKey("Sender")]
            public string SenderID { get; set; }

            [Required]
            [StringLength(2000)]
            public string Content { get; set; }

            [Required]
            public DateTime SentDate { get; set; } = DateTime.Now;

            public virtual Message Message { get; set; }
            public virtual Users Sender { get; set; }
        }
 }