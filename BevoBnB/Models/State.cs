using System.ComponentModel.DataAnnotations;

namespace BevoBnB.Models
{
    public class State
    {
        public int StateID { get; set; }

        [Required]
        [StringLength(2)]
        public string StateCode { get; set; }

        [Required]
        [StringLength(50)]
        public string StateName { get; set; }
    }
}
