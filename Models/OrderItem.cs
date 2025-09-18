using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRISM.Models
{
    public class OrderItem
    {


        //OrderItem
        //order_item_id (PK), order_id (FK), product_id (FK), qty, unit_price, discount
        [Key]
        public int OrderItemId { get; set; }
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order order { get; set; }
        public decimal TotalPrice { get; set; }


        [ForeignKey("Item")]
        public int ItemId { get; set; }
        public Item Item { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }


    }
}
