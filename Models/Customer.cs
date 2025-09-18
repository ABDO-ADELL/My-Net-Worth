using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRISM.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }


        [ForeignKey("Branch")]
        public int BranchId { get; set; }
        public Branch Branch { get; set; }
    }
}
