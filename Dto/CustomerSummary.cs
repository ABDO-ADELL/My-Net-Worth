namespace PRISM.Dto
{
    public class CustomerSummary
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
