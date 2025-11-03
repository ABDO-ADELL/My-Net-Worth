using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRISM
{
    public class Business
    {
        [Key]
        public int BusinessId { get; set; }
        public string Name { get; set; }
        public string Industry { get; set; }
        public string Timezone { get; set; }
        public string Status { get; set; }
        public bool IsDeleted { get; set; }

        // Relations
        public ICollection<Branch> Branches { get; set; }
        public ICollection<Items> Items { get; set; }
    }

}
