using System;
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
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }


        // Navigation
        [ForeignKey("Business")]
        public int BusinessId { get; set; }
        public Business Business { get; set; }  
        public ICollection<Items> Items { get; set; }
        public ICollection<Inventory> Inventories { get; set; }
    }

}
