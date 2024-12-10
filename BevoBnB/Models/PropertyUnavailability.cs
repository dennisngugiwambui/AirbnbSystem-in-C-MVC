using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BevoBnB.Models
{
    public class PropertyUnavailability
    {
        [Required]
        [ForeignKey("Property")]
        public int PropertyID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Unavailable Date")]
        [Column(TypeName = "date")]
        public DateTime UnavailableDate { get; set; }

        [Display(Name = "Reason")]
        [StringLength(100)]
        public string? Reason { get; set; }

        [Required]
        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Added By")]
        public string AddedByUserID { get; set; }

        // Navigation property
        public virtual Property Property { get; set; }
    }
}
