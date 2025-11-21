using PRISM.Controllers;
using PRISM.Dto;

namespace PRISM.Models
{
    public class ReportViewModel
    {
        public int? BusinessId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ReportType { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalOrders { get; set; }
        public int TotalPayments { get; set; }
        public decimal AverageOrderValue { get; set; }

        public List<Order> Orders { get; set; } = new();
        public List<Expense> Expenses { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();

        public List<BusinessPerformance> BusinessPerformance { get; set; } = new();
        public List<CategorySummary> ExpensesByCategory { get; set; } = new();
        public List<TopSellingItem> TopSellingItems { get; set; } = new();
        public List<CustomerSummary> TopCustomers { get; set; } = new();
    }
}
