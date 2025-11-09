using PRISM.Controllers;

namespace PRISM.Models
{
    public class DashboardViewModel
    {
        public int BusinessId { get; set; }
        public bool IsAllBusinesses { get; set; } 
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalItems { get; set; }
        public int TotalBranches { get; set; }

        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<Expense> RecentExpenses { get; set; } = new List<Expense>();
        public List<MonthlyData> MonthlyRevenue { get; set; } = new List<MonthlyData>();
        public List<MonthlyData> MonthlyExpenses { get; set; } = new List<MonthlyData>();
        public List<TopSellingItem> TopSellingItems { get; set; } = new List<TopSellingItem>();
        public List<Inventory> LowStockItems { get; set; } = new List<Inventory>();

    }
}
