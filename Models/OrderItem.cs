using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRISM.Models
{
    public class OrderItem
    {

        [Key]
        public int OrderItemId { get; set; }
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order order { get; set; }

        public decimal TotalPrice { get; set; }
        [ForeignKey("Item")]
        public int ItemId { get; set; }
        public Items Item { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }


    }
}
