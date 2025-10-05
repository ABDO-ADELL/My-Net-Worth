using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRISM.Models
{
    public class Expense
    {
        public int ExpenseId { get; set; }

        public int BusinessId { get; set; }

        public int? BranchId { get; set; }

        public int CategoryId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }



        [ForeignKey(nameof(CategoryId))]
        public virtual ExpenseCategory ExpenseCategorys { get; set; }

        [ForeignKey(nameof(BusinessId))]
        public virtual Business Business { get; set; }

        [ForeignKey(nameof(BranchId))]
        public virtual Branch? Branch { get; set; }
    }
}
