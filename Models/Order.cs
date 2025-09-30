using PRISM.Models.Authmodels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRISM.Models
{
    public class Order
    {

        //Order
        //order_id(PK), business_id(FK), branch_id(FK), customer_id(FK), user_id(FK), datetime, total_amount, status
        [Key]
        public int order_id { get; set; }
        public string OrderName { get; set; }
        public int business_id { get; set; }
        public Business business { get; set; }
        public int BranchId { get; set; }
        public Branch branch { get; set; }
        public int UserId { get; set; }
        public AppUser user { get; set; }
        public DateTime datetime { get; set; }
        public decimal total_amount { get; set; }
        public bool status { get; set; }
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }

        public bool IsDeleted { get; set; }



    }
}
