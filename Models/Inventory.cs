using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRISM
{
    public class Inventory
    {
        [Key]
        public int InventoryId { get; set; }

        [Required]
        [ForeignKey("Branch")]
        public int BranchId { get; set; }
        public Branch? Branch { get; set; } 

        [Required]
        [ForeignKey("Item")]
        public int ItemId { get; set; }
        public Items? Item { get; set; } 

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int MinStockLevel { get; set; }

        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    }

}
