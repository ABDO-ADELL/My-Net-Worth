using System.ComponentModel.DataAnnotations;

namespace PRISM.Models
{
    public class ExpenseCategory
    {
        [Key]
        public int ExpenseCategoryId { get; set; }
        public int BusinessId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }

         public Business Business { get; set; } = null!;
         public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
