using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BevoBnB.Models
{
    public enum DisputeStatus
    {
        None = 0,
        Disputed = 1,
        Resolved = 2,
        Removed = 3
    }

    public class Review
    {
        [Key]
        public int ReviewID { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public string CustomerID { get; set; }

       
        [Required]
        [ForeignKey("Property")]
        public int PropertyID { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(280)]
        [Display(Name = "Review")]
        public string? ReviewText { get; set; }

        [StringLength(280)]
        [Display(Name = "Host Response")]
        public string? HostComments { get; set; }

        [Required]
        public DisputeStatus DisputeStatus { get; set; } = DisputeStatus.None;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Users Customer { get; set; }
        public virtual Property Property { get; set; }
    }
}