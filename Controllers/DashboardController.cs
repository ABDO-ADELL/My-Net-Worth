using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRISM.Models;

namespace PRISM.Controllers
{
    [AllowAnonymous]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? businessId)
        {
            // Check if "All Businesses" option is selected (businessId = 0)
            bool showAllBusinesses = businessId.HasValue && businessId.Value == 0;

            // If no business selected, get the first one or allow selection
            if (!businessId.HasValue)
            {
                var firstBusiness = await _context.Businesses
                    .Where(b => !b.IsDeleted)
                    .FirstOrDefaultAsync();

                if (firstBusiness != null)
                {
                    businessId = firstBusiness.BusinessId;
                }
                else
                {
                    // No businesses exist
                    ViewBag.NoBusiness = true;
                    return View();
                }
            }

            var viewModel = new DashboardViewModel
            {
                BusinessId = businessId.Value,
                IsAllBusinesses = showAllBusinesses,

                // Total Revenue (from completed orders)
                TotalRevenue = showAllBusinesses
                    ? await _context.Orders
                        .Where(o => !o.IsDeleted && o.status)
                        .SumAsync(o => o.total_amount)
                    : await _context.Orders
                        .Where(o => o.BusinessId == businessId.Value && !o.IsDeleted && o.status)
                        .SumAsync(o => o.total_amount),

                // Total Expenses
                TotalExpenses = showAllBusinesses
                    ? await _context.Expenses
                        .Where(e => !e.IsDeleted)
                        .SumAsync(e => e.Amount)
                    : await _context.Expenses
                        .Where(e => e.BusinessId == businessId.Value && !e.IsDeleted)
                        .SumAsync(e => e.Amount),

                // Total Orders Count
                TotalOrders = showAllBusinesses
                    ? await _context.Orders
                        .Where(o => !o.IsDeleted)
                        .CountAsync()
                    : await _context.Orders
                        .Where(o => o.BusinessId == businessId.Value && !o.IsDeleted)
                        .CountAsync(),

                // Total Customers Count
                TotalCustomers = showAllBusinesses
                    ? await _context.Customers.CountAsync()
                    : await _context.Customers
                        .Include(c => c.Branch)
                        .Where(c => c.Branch.BusinessId == businessId.Value)
                        .CountAsync(),

                // Total Active Items
                TotalItems = showAllBusinesses
                    ? await _context.Items
                        .Where(i => !i.IsDeleted)
                        .CountAsync()
                    : await _context.Items
                        .Where(i => i.BusinessId == businessId.Value && !i.IsDeleted)
                        .CountAsync(),

                // Total Branches
                TotalBranches = showAllBusinesses
                    ? await _context.Branches
                        .Where(b => !b.IsDeleted)
                        .CountAsync()
                    : await _context.Branches
                        .Where(b => b.BusinessId == businessId.Value && !b.IsDeleted)
                        .CountAsync(),

                // Recent Orders (Last 5)
                RecentOrders = showAllBusinesses
                    ? await _context.Orders
                        .Include(o => o.Customer)
                        .Include(o => o.branch)
                        .Include(o => o.business)
                        .Where(o => !o.IsDeleted)
                        .OrderByDescending(o => o.datetime)
                        .Take(5)
                        .ToListAsync()
                    : await _context.Orders
                        .Include(o => o.Customer)
                        .Include(o => o.branch)
                        .Where(o => o.BusinessId == businessId.Value && !o.IsDeleted)
                        .OrderByDescending(o => o.datetime)
                        .Take(5)
                        .ToListAsync(),

                // Monthly Revenue (Last 6 months)
                MonthlyRevenue = await GetMonthlyRevenue(showAllBusinesses ? null : businessId.Value),

                // Monthly Expenses (Last 6 months)
                MonthlyExpenses = await GetMonthlyExpenses(showAllBusinesses ? null : businessId.Value),

                // Top Selling Items (Top 5)
                TopSellingItems = await GetTopSellingItems(showAllBusinesses ? null : businessId.Value),

                // Recent Expenses (Last 5)
                RecentExpenses = showAllBusinesses
                    ? await _context.Expenses
                        .Include(e => e.ExpenseCategorys)
                        .Include(e => e.Branch)
                        .Include(e => e.Business)
                        .Where(e => !e.IsDeleted)
                        .OrderByDescending(e => e.ExpenseDate)
                        .Take(5)
                        .ToListAsync()
                    : await _context.Expenses
                        .Include(e => e.ExpenseCategorys)
                        .Include(e => e.Branch)
                        .Where(e => e.BusinessId == businessId.Value && !e.IsDeleted)
                        .OrderByDescending(e => e.ExpenseDate)
                        .Take(5)
                        .ToListAsync(),

                // Low Stock Items (if quantity < MinStockLevel)
                LowStockItems = showAllBusinesses
                    ? await _context.Inventories
                        .Include(i => i.Item)
                        .Include(i => i.Branch)
                            .ThenInclude(b => b.Business)
                        .Where(i => i.Quantity < i.MinStockLevel)
                        .OrderBy(i => i.Quantity)
                        .Take(5)
                        .ToListAsync()
                    : await _context.Inventories
                        .Include(i => i.Item)
                        .Include(i => i.Branch)
                        .Where(i => i.Branch.BusinessId == businessId.Value
                               && i.Quantity < i.MinStockLevel)
                        .OrderBy(i => i.Quantity)
                        .Take(5)
                        .ToListAsync()
            };

            // Calculate profit
            viewModel.TotalProfit = viewModel.TotalRevenue - viewModel.TotalExpenses;

            // Get business list for dropdown
            ViewBag.Businesses = await _context.Businesses
                .Where(b => !b.IsDeleted)
                .Select(b => new { b.BusinessId, b.Name })
                .ToListAsync();

            ViewBag.SelectedBusinessId = businessId.Value;

            return View(viewModel);
        }

        private async Task<List<MonthlyData>> GetMonthlyRevenue(int? businessId)
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            var query = _context.Orders
                .Where(o => !o.IsDeleted
                       && o.status
                       && o.datetime >= sixMonthsAgo);

            // Apply business filter if specified
            if (businessId.HasValue)
            {
                query = query.Where(o => o.BusinessId == businessId.Value);
            }

            var monthlyData = await query
                .GroupBy(o => new { o.datetime.Year, o.datetime.Month })
                .Select(g => new MonthlyData
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(o => o.total_amount)
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToListAsync();

            return monthlyData;
        }

        private async Task<List<MonthlyData>> GetMonthlyExpenses(int? businessId)
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            var query = _context.Expenses
                .Where(e => !e.IsDeleted
                       && e.ExpenseDate >= sixMonthsAgo);

            // Apply business filter if specified
            if (businessId.HasValue)
            {
                query = query.Where(e => e.BusinessId == businessId.Value);
            }

            var monthlyData = await query
                .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
                .Select(g => new MonthlyData
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(e => e.Amount)
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToListAsync();

            return monthlyData;
        }

        private async Task<List<TopSellingItem>> GetTopSellingItems(int? businessId)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Item)
                .Include(oi => oi.order)
                .Where(oi => !oi.order.IsDeleted && oi.order.status);

            // Apply business filter if specified
            if (businessId.HasValue)
            {
                query = query.Where(oi => oi.order.BusinessId == businessId.Value);
            }

            var topItems = await query
                .GroupBy(oi => new { oi.ItemId, oi.Item.Name })
                .Select(g => new TopSellingItem
                {
                    ItemId = g.Key.ItemId,
                    ItemName = g.Key.Name,
                    TotalQuantitySold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(i => i.TotalQuantitySold)
                .Take(5)
                .ToListAsync();

            return topItems;
        }
    }
}