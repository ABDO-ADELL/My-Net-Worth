using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRISM.Models
{
    public class Target
    {
        [Key]
        public int TargetId { get; set; }

        // You need these foreign key properties:
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public Business business { get; set; }

        // Your other properties are fine
        public DateTime period_start { get; set; }
        public DateTime period_end { get; set; }
        public decimal target_revenue { get; set; }
        public decimal? target_profit { get; set; }
        public DateTime created_at { get; set; }
    }

}

