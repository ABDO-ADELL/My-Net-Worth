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
        public int BranchId { get; set; }
        public int MinStockLevel { get; set; }
        public DateTime LastUpdate { get; set; }

        // Navigation
        public Branch Branch { get; set; }
        [ForeignKey("Item")]
        public int ItemId { get; set; }
        public Item Item { get; set; }

        public int Quantity { get; set; }
    }

}
