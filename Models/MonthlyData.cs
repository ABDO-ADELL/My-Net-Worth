namespace PRISM.Models
{
    public class MonthlyData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }

        public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");


    }
}
