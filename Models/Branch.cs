using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRISM
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public bool IsDeleted { get; set; }


        // Navigation
        [ForeignKey("Business")]
        public int BusinessId { get; set; }
        public Business Business { get; set; }
        public ICollection<Item> Items { get; set; }
        public ICollection<Inventory> Inventories { get; set; }
    }

}
