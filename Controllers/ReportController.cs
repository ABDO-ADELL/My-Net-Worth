using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRISM.Dto;
using System.Drawing;
using System.Security.Claims;

namespace PRISM.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Report/Index
        public async Task<IActionResult> Index(int? businessId, DateTime? startDate, DateTime? endDate, string reportType = "summary")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get user's businesses
            var userBusinesses = await _context.Businesses
                .Where(b => !b.IsDeleted && b.UserId == userId)
                .ToListAsync();

            if (!userBusinesses.Any())
            {
                ViewBag.NoBusiness = true;
                ViewBag.Businesses = new List<Business>();
                return View(new ReportViewModel());
            }

            // Set date range defaults
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Now;

            // Build query based on business selection
            IQueryable<Order> ordersQuery = _context.Orders
                .Include(o => o.business)
                .Include(o => o.branch)
                .Include(o => o.Customer)
                .Include(o => o.user)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Where(o => !o.IsDeleted
                    && o.datetime >= startDate.Value
                    && o.datetime <= endDate.Value
                    && o.business.UserId == userId);

            IQueryable<Expense> expensesQuery = _context.Expenses
                .Include(e => e.Business)
                .Include(e => e.Branch)
                .Where(e => !e.IsDeleted
                    && e.ExpenseDate >= startDate.Value
                    && e.ExpenseDate <= endDate.Value
                    && e.Business.UserId == userId);

            IQueryable<Payment> paymentsQuery = _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.business)
                .Where(p => !p.IsDeleted
                    && p.datetime >= startDate.Value
                    && p.datetime <= endDate.Value
                    && p.Order.business.UserId == userId);

            // Filter by specific business if selected
            if (businessId.HasValue && businessId.Value > 0)
            {
                ordersQuery = ordersQuery.Where(o => o.BusinessId == businessId.Value);
                expensesQuery = expensesQuery.Where(e => e.BusinessId == businessId.Value);
                paymentsQuery = paymentsQuery.Where(p => p.Order.BusinessId == businessId.Value);
            }

            var orders = await ordersQuery.ToListAsync();
            var expenses = await expensesQuery.ToListAsync();
            var payments = await paymentsQuery.ToListAsync();

            var viewModel = new ReportViewModel
            {
                BusinessId = businessId,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                ReportType = reportType,

                // Summary Data
                TotalRevenue = orders.Sum(o => o.total_amount),
                TotalExpenses = expenses.Sum(e => e.Amount),
                TotalOrders = orders.Count,
                TotalPayments = payments.Count,
                AverageOrderValue = orders.Any() ? orders.Average(o => o.total_amount) : 0,

                // Detailed Data
                Orders = orders,
                Expenses = expenses,
                Payments = payments,

                // Business Performance
                BusinessPerformance = await GetBusinessPerformance(userId, startDate.Value, endDate.Value, businessId),

                // Category Breakdown
                ExpensesByCategory = expenses
                    .GroupBy(e => e.Category ?? "Uncategorized")
                    .Select(g => new CategorySummary
                    {
                        Category = g.Key,
                        TotalAmount = g.Sum(e => e.Amount),
                        Count = g.Count()
                    })
                    .OrderByDescending(c => c.TotalAmount)
                    .ToList(),

                // Top Selling Items
                TopSellingItems = await GetTopSellingItems(userId, startDate.Value, endDate.Value, businessId),

                // Customer Analysis
                TopCustomers = await GetTopCustomers(userId, startDate.Value, endDate.Value, businessId)
            };

            viewModel.TotalProfit = viewModel.TotalRevenue - viewModel.TotalExpenses;

            ViewBag.Businesses = userBusinesses;
            ViewBag.SelectedBusinessId = businessId;

            return View(viewModel);
        }

        // Export to Excel
        public async Task<IActionResult> ExportToExcel(int? businessId, DateTime? startDate, DateTime? endDate)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Now;

            // Get data
            IQueryable<Order> ordersQuery = _context.Orders
                .Include(o => o.business)
                .Include(o => o.branch)
                .Include(o => o.Customer)
                .Include(o => o.user)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Where(o => !o.IsDeleted
                    && o.datetime >= startDate.Value
                    && o.datetime <= endDate.Value
                    && o.business.UserId == userId);

            IQueryable<Expense> expensesQuery = _context.Expenses
                .Include(e => e.Business)
                .Include(e => e.Branch)
                .Where(e => !e.IsDeleted
                    && e.ExpenseDate >= startDate.Value
                    && e.ExpenseDate <= endDate.Value
                    && e.Business.UserId == userId);

            IQueryable<Payment> paymentsQuery = _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.business)
                .Where(p => !p.IsDeleted
                    && p.datetime >= startDate.Value
                    && p.datetime <= endDate.Value
                    && p.Order.business.UserId == userId);

            if (businessId.HasValue && businessId.Value > 0)
            {
                ordersQuery = ordersQuery.Where(o => o.BusinessId == businessId.Value);
                expensesQuery = expensesQuery.Where(e => e.BusinessId == businessId.Value);
                paymentsQuery = paymentsQuery.Where(p => p.Order.BusinessId == businessId.Value);
            }

            var orders = await ordersQuery.ToListAsync();
            var expenses = await expensesQuery.ToListAsync();
            var payments = await paymentsQuery.ToListAsync();

            // Create Excel file
            using var workbook = new XLWorkbook();

            // Summary Sheet
            var summarySheet = workbook.Worksheets.Add("Summary");
            CreateSummarySheet(summarySheet, orders, expenses, payments, startDate.Value, endDate.Value);

            // Orders Sheet
            var ordersSheet = workbook.Worksheets.Add("Orders");
            CreateOrdersSheet(ordersSheet, orders);

            // Expenses Sheet
            var expensesSheet = workbook.Worksheets.Add("Expenses");
            CreateExpensesSheet(expensesSheet, expenses);

            // Payments Sheet
            var paymentsSheet = workbook.Worksheets.Add("Payments");
            CreatePaymentsSheet(paymentsSheet, payments);

            // Order Items Sheet
            var orderItemsSheet = workbook.Worksheets.Add("Order Items");
            CreateOrderItemsSheet(orderItemsSheet, orders);

            // Save to memory stream
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            var fileName = $"Report_{startDate.Value:yyyy-MM-dd}_to_{endDate.Value:yyyy-MM-dd}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // Helper Methods
        private void CreateSummarySheet(IXLWorksheet sheet, List<Order> orders, List<Expense> expenses,
            List<Payment> payments, DateTime startDate, DateTime endDate)
        {
            sheet.Cell(1, 1).Value = "PRISM Financial Report";
            sheet.Cell(1, 1).Style.Font.Bold = true;
            sheet.Cell(1, 1).Style.Font.FontSize = 16;

            sheet.Cell(2, 1).Value = $"Period: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}";
            sheet.Cell(2, 1).Style.Font.Italic = true;

            int row = 4;
            sheet.Cell(row, 1).Value = "Metric";
            sheet.Cell(row, 2).Value = "Value";
            sheet.Range(row, 1, row, 2).Style.Font.Bold = true;
            sheet.Range(row, 1, row, 2).Style.Fill.BackgroundColor = XLColor.LightGray;

            row++;
            sheet.Cell(row++, 1).Value = "Total Orders";
            sheet.Cell(row - 1, 2).Value = orders.Count;

            sheet.Cell(row++, 1).Value = "Total Revenue";
            sheet.Cell(row - 1, 2).Value = orders.Sum(o => o.total_amount);
            sheet.Cell(row - 1, 2).Style.NumberFormat.Format = "$#,##0.00";

            sheet.Cell(row++, 1).Value = "Total Expenses";
            sheet.Cell(row - 1, 2).Value = expenses.Sum(e => e.Amount);
            sheet.Cell(row - 1, 2).Style.NumberFormat.Format = "$#,##0.00";

            sheet.Cell(row++, 1).Value = "Net Profit";
            sheet.Cell(row - 1, 2).Value = orders.Sum(o => o.total_amount) - expenses.Sum(e => e.Amount);
            sheet.Cell(row - 1, 2).Style.NumberFormat.Format = "$#,##0.00";
            sheet.Cell(row - 1, 2).Style.Font.Bold = true;

            sheet.Cell(row++, 1).Value = "Total Payments";
            sheet.Cell(row - 1, 2).Value = payments.Sum(p => p.Amount);
            sheet.Cell(row - 1, 2).Style.NumberFormat.Format = "$#,##0.00";

            sheet.Cell(row++, 1).Value = "Average Order Value";
            sheet.Cell(row - 1, 2).Value = orders.Any() ? orders.Average(o => o.total_amount) : 0;
            sheet.Cell(row - 1, 2).Style.NumberFormat.Format = "$#,##0.00";

            sheet.Columns().AdjustToContents();
        }

        private void CreateOrdersSheet(IXLWorksheet sheet, List<Order> orders)
        {
            sheet.Cell(1, 1).Value = "Order ID";
            sheet.Cell(1, 2).Value = "Order Name";
            sheet.Cell(1, 3).Value = "Business";
            sheet.Cell(1, 4).Value = "Branch";
            sheet.Cell(1, 5).Value = "Customer";
            sheet.Cell(1, 6).Value = "Date";
            sheet.Cell(1, 7).Value = "Total Amount";
            sheet.Cell(1, 8).Value = "Status";
            sheet.Cell(1, 9).Value = "Created By";

            sheet.Range(1, 1, 1, 9).Style.Font.Bold = true;
            sheet.Range(1, 1, 1, 9).Style.Fill.BackgroundColor = XLColor.LightBlue;

            int row = 2;
            foreach (var order in orders)
            {
                sheet.Cell(row, 1).Value = order.Id;
                sheet.Cell(row, 2).Value = order.OrderName;
                sheet.Cell(row, 3).Value = order.business?.Name;
                sheet.Cell(row, 4).Value = order.branch?.Name;
                sheet.Cell(row, 5).Value = order.Customer?.FullName;
                sheet.Cell(row, 6).Value = order.datetime;
                sheet.Cell(row, 6).Style.DateFormat.Format = "mm/dd/yyyy hh:mm";
                sheet.Cell(row, 7).Value = order.total_amount;
                sheet.Cell(row, 7).Style.NumberFormat.Format = "$#,##0.00";
                sheet.Cell(row, 8).Value = order.status ? "Active" : "Inactive";
                sheet.Cell(row, 9).Value = order.user?.UserName;
                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        private void CreateExpensesSheet(IXLWorksheet sheet, List<Expense> expenses)
        {
            sheet.Cell(1, 1).Value = "Expense ID";
            sheet.Cell(1, 2).Value = "Business";
            sheet.Cell(1, 3).Value = "Branch";
            sheet.Cell(1, 4).Value = "Category";
            sheet.Cell(1, 5).Value = "Amount";
            sheet.Cell(1, 6).Value = "Date";
            sheet.Cell(1, 7).Value = "Payment Method";
            sheet.Cell(1, 8).Value = "Description";

            sheet.Range(1, 1, 1, 8).Style.Font.Bold = true;
            sheet.Range(1, 1, 1, 8).Style.Fill.BackgroundColor = XLColor.LightCoral;

            int row = 2;
            foreach (var expense in expenses)
            {
                sheet.Cell(row, 1).Value = expense.ExpenseId;
                sheet.Cell(row, 2).Value = expense.Business?.Name;
                sheet.Cell(row, 3).Value = expense.Branch?.Name ?? "General";
                sheet.Cell(row, 4).Value = expense.Category ?? "Uncategorized";
                sheet.Cell(row, 5).Value = expense.Amount;
                sheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
                sheet.Cell(row, 6).Value = expense.ExpenseDate;
                sheet.Cell(row, 6).Style.DateFormat.Format = "mm/dd/yyyy";
                sheet.Cell(row, 7).Value = expense.PaymentMethod;
                sheet.Cell(row, 8).Value = expense.Description;
                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        private void CreatePaymentsSheet(IXLWorksheet sheet, List<Payment> payments)
        {
            sheet.Cell(1, 1).Value = "Payment ID";
            sheet.Cell(1, 2).Value = "Order ID";
            sheet.Cell(1, 3).Value = "Business";
            sheet.Cell(1, 4).Value = "Amount";
            sheet.Cell(1, 5).Value = "Method";
            sheet.Cell(1, 6).Value = "Date";

            sheet.Range(1, 1, 1, 6).Style.Font.Bold = true;
            sheet.Range(1, 1, 1, 6).Style.Fill.BackgroundColor = XLColor.LightGreen;

            int row = 2;
            foreach (var payment in payments)
            {
                sheet.Cell(row, 1).Value = payment.PaymentId;
                sheet.Cell(row, 2).Value = payment.OrderId;
                sheet.Cell(row, 3).Value = payment.Order?.business?.Name;
                sheet.Cell(row, 4).Value = payment.Amount;
                sheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
                sheet.Cell(row, 5).Value = payment.Method;
                sheet.Cell(row, 6).Value = payment.datetime;
                sheet.Cell(row, 6).Style.DateFormat.Format = "mm/dd/yyyy hh:mm";
                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        private void CreateOrderItemsSheet(IXLWorksheet sheet, List<Order> orders)
        {
            sheet.Cell(1, 1).Value = "Order ID";
            sheet.Cell(1, 2).Value = "Order Name";
            sheet.Cell(1, 3).Value = "Item Name";
            sheet.Cell(1, 4).Value = "Quantity";
            sheet.Cell(1, 5).Value = "Unit Price";
            sheet.Cell(1, 6).Value = "Total Price";

            sheet.Range(1, 1, 1, 6).Style.Font.Bold = true;
            sheet.Range(1, 1, 1, 6).Style.Fill.BackgroundColor = XLColor.LightYellow;

            int row = 2;
            foreach (var order in orders)
            {
                if (order.OrderItems != null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        sheet.Cell(row, 1).Value = order.Id;
                        sheet.Cell(row, 2).Value = order.OrderName;
                        sheet.Cell(row, 3).Value = item.Item?.Name;
                        sheet.Cell(row, 4).Value = item.Quantity;
                        sheet.Cell(row, 5).Value = item.Price;
                        sheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
                        sheet.Cell(row, 6).Value = item.TotalPrice;
                        sheet.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";
                        row++;
                    }
                }
            }

            sheet.Columns().AdjustToContents();
        }

        private async Task<List<BusinessPerformance>> GetBusinessPerformance(string userId, DateTime startDate,
            DateTime endDate, int? businessId)
        {
            var query = _context.Orders
                .Include(o => o.business)
                .Where(o => !o.IsDeleted
                    && o.datetime >= startDate
                    && o.datetime <= endDate
                    && o.business.UserId == userId);

            if (businessId.HasValue && businessId.Value > 0)
                query = query.Where(o => o.BusinessId == businessId.Value);

            var businessPerf = await query
                .GroupBy(o => new { o.BusinessId, o.business.Name })
                .Select(g => new BusinessPerformance
                {
                    BusinessId = g.Key.BusinessId,
                    BusinessName = g.Key.Name,
                    TotalOrders = g.Count(),
                    TotalRevenue = g.Sum(o => o.total_amount),
                    AverageOrderValue = g.Average(o => o.total_amount)
                })
                .ToListAsync();

            // Add expenses
            foreach (var perf in businessPerf)
            {
                perf.TotalExpenses = await _context.Expenses
                    .Where(e => e.BusinessId == perf.BusinessId
                        && !e.IsDeleted
                        && e.ExpenseDate >= startDate
                        && e.ExpenseDate <= endDate)
                    .SumAsync(e => e.Amount);
            }

            return businessPerf;
        }

        private async Task<List<TopSellingItem>> GetTopSellingItems(string userId, DateTime startDate,
            DateTime endDate, int? businessId)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Item)
                .Include(oi => oi.order)
                    .ThenInclude(o => o.business)
                .Where(oi => !oi.order.IsDeleted
                    && oi.order.datetime >= startDate
                    && oi.order.datetime <= endDate
                    && oi.order.business.UserId == userId);

            if (businessId.HasValue && businessId.Value > 0)
                query = query.Where(oi => oi.order.BusinessId == businessId.Value);

            return await query
                .GroupBy(oi => new { oi.ItemId, oi.Item.Name })
                .Select(g => new TopSellingItem
                {
                    ItemId = g.Key.ItemId,
                    ItemName = g.Key.Name,
                    TotalQuantitySold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(i => i.TotalQuantitySold)
                .Take(10)
                .ToListAsync();
        }

        private async Task<List<CustomerSummary>> GetTopCustomers(string userId, DateTime startDate,
            DateTime endDate, int? businessId)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.business)
                .Where(o => !o.IsDeleted
                    && o.datetime >= startDate
                    && o.datetime <= endDate
                    && o.business.UserId == userId);

            if (businessId.HasValue && businessId.Value > 0)
                query = query.Where(o => o.BusinessId == businessId.Value);

            return await query
                .GroupBy(o => new { o.CustomerId, o.Customer.FullName })
                .Select(g => new CustomerSummary
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = g.Key.FullName,
                    TotalOrders = g.Count(),
                    TotalSpent = g.Sum(o => o.total_amount)
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(10)
                .ToListAsync();
        }
    }
}