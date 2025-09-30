using PRISM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRISM
{
    public class Item
    {

        [Key]
        public int ItemId { get; set; }
        public int BusinessId { get; set; }
        public int? BranchId { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }  // Stock Keeping Unit
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
        public int? DurationMinutes { get; set; } // for services
        public string Description { get; set; }

        public bool IsDeleted { get; set; } 

        // Navigation
        public Business Business { get; set; }
        public Branch Branch { get; set; }
        public ICollection<Inventory> Inventories { get; set; }
        [ForeignKey("ItemCategory")]
        public int CategoryId { get; set; }
        public ItemCategory ItemCategory { get; set; }
    }

}
