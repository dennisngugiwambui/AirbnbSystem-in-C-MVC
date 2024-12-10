using BevoBnB.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Category
{
    public Category()
    {
        Properties = new HashSet<Property>();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CategoryID { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Category Name")]
    public string CategoryName { get; set; }

    // Navigation property
    public virtual ICollection<Property> Properties { get; set; }
}