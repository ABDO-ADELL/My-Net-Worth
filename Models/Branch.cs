using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRISM.Models
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "Branch name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string Phone { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;

        // ✅ FIX: BusinessId must NOT be nullable for required relationship
        [Required(ErrorMessage = "Business is required")]
        [ForeignKey("Business")]
        public int BusinessId { get; set; }

        // Navigation property
        public Business? Business { get; set; }
        public ICollection<Items> Items { get; set; }
        public ICollection<Inventory> Inventories { get; set; }
    }

}
