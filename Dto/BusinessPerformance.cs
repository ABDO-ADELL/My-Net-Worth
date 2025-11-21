namespace PRISM.Dto
{
    public class BusinessPerformance
    {
        public int BusinessId { get; set; }
        public string BusinessName { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal AverageOrderValue { get; set; }

    }
}
