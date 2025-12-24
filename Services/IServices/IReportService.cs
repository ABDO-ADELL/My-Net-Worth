using ClosedXML.Excel;
using PRISM.Dto;

namespace PRISM.Services.IServices
{
    public interface IReportService
    {
        void CreateBranchesSheet(IXLWorksheet sheet, List<Branch> branches);
        void CreateBusinessesSheet(IXLWorksheet sheet, List<Business> businesses);
        void CreateCategoriesSheet(IXLWorksheet sheet, List<ItemCategory> categories);
        void CreateCustomersSheet(IXLWorksheet sheet, List<Customer> customers);
        void CreateExpensesSheet(IXLWorksheet sheet, List<Expense> expenses);
        void CreateItemsSheet(IXLWorksheet sheet, List<Items> items);
        void CreateOrderItemsSheet(IXLWorksheet sheet, List<Order> orders);
        void CreateOrdersSheet(IXLWorksheet sheet, List<Order> orders);
        void CreatePaymentsSheet(IXLWorksheet sheet, List<Payment> payments);
        void CreateSummarySheet(IXLWorksheet sheet, List<Order> orders, List<Expense> expenses, List<Payment> payments, DateTime startDate, DateTime endDate);
        void CreateSuppliersSheet(IXLWorksheet sheet, List<Supplier> suppliers);
        void CreateSupplierSummarySheet(IXLWorksheet sheet, List<SupplierSummary> summaries);
        Task<List<BusinessPerformance>> GetBusinessPerformance(string userId, DateTime startDate, DateTime endDate, int? businessId);
        Task<List<SupplierSummary>> GetSupplierSummary(string userId);
        Task<List<CustomerSummary>> GetTopCustomers(string userId, DateTime startDate, DateTime endDate, int? businessId);
        Task<List<TopSellingItem>> GetTopSellingItems(string userId, DateTime startDate, DateTime endDate, int? businessId);
        Task<ReportViewModel> GetIndexAsync(string userId, int? businessId, DateTime? startDate, DateTime? endDate, string reportType = "summary");
        Task<ExcelFileResult> ExportToExcelAsync(string userId, int? businessId, DateTime? startDate, DateTime? endDate, string? reportType);

    }
}