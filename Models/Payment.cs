using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRISM.Models
{
    public class Payment
    {

        //Payment
        //payment_id (PK), order_id (FK), method, amount, datetime
        [Key]
        public int PaymentId { get; set; }
        public string Method { get; set; }
        public decimal Amount { get; set; }
        public DateTime datetime { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

    }
}
