using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRISM.DataAccess;
using PRISM.Models;
using System.Security.Claims;

namespace PRISM.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? businessId)
        {
            // Get all active businesses for dropdown
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var businesses = await _context.Businesses
                .Where(b => !b.IsDeleted && b.UserId == userId)
                .Select(b => new { b.BusinessId, b.Name })
                .ToListAsync();

            // Check if any businesses exist
            if (!businesses.Any())
            {
                ViewBag.NoBusiness = true;
                ViewBag.Businesses = new List<dynamic>();
                return View(new DashboardViewModel());
            }

            // If no business selected, get the first one
            if (!businessId.HasValue)
            {
                businessId = businesses.First().BusinessId;
            }

            // Verify the selected business exists
            if (!businesses.Any(b => b.BusinessId == businessId.Value))
            {
                businessId = businesses.First().BusinessId;
            }

            // Store selected business in TempData for other pages to use
            TempData["SelectedBusinessId"] = businessId.Value;
            TempData.Keep("SelectedBusinessId");

            var viewModel = new DashboardViewModel
            {
                BusinessId = businessId.Value,

                // Total Revenue (from completed orders)
                TotalRevenue = await _context.Orders
                    .Where(o => o.BusinessId == businessId.Value && !o.IsDeleted && o.status)
                    .SumAsync(o => o.total_amount),

                // Total Expenses
                TotalExpenses = await _context.Expenses
                    .Where(e => e.BusinessId == businessId.Value && !e.IsDeleted)
                    .SumAsync(e => e.Amount),

                // Total Orders Count
                TotalOrders = await _context.Orders
                    .Where(o => o.BusinessId == businessId.Value && !o.IsDeleted)
                    .CountAsync(),

                // Total Customers Count
                TotalCustomers = await _context.Customers
                    .Include(c => c.Branch)
                    .Where(c => c.Branch.BusinessId == businessId.Value)
                    .CountAsync(),

                // Total Active Items
                TotalItems = await _context.Items
                    .Where(i => i.BusinessId == businessId.Value && !i.IsDeleted)
                    .CountAsync(),

                // Total Branches
                TotalBranches = await _context.Branches
                    .Where(b => b.BusinessId == businessId.Value && !b.IsDeleted)
                    .CountAsync(),

                // Recent Orders (Last 5)
                RecentOrders = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.branch)
                    .Where(o => o.BusinessId == businessId.Value && !o.IsDeleted)
                    .OrderByDescending(o => o.datetime)
                    .Take(5)
                    .ToListAsync(),

                // Recent Expenses (Last 5) - Added this
                RecentExpenses = await _context.Expenses
                    .Include(e => e.Branch)
                    .Where(e => e.BusinessId == businessId.Value && !e.IsDeleted)
                    .OrderByDescending(e => e.ExpenseDate)
                    .Take(5)
                    .ToListAsync(),

                // Monthly Revenue (Last 6 months)
                MonthlyRevenue = await GetMonthlyRevenue(businessId.Value),

                // Monthly Expenses (Last 6 months)
                MonthlyExpenses = await GetMonthlyExpenses(businessId.Value),

                // Top Selling Items (Top 5)
                TopSellingItems = await GetTopSellingItems(businessId.Value),

                // Low Stock Items (if quantity < MinStockLevel)
                LowStockItems = await _context.Inventories
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
                .Where(b => !b.IsDeleted && b.UserId == userId)
                .Select(b => new { b.BusinessId, b.Name })
                .ToListAsync();

            ViewBag.SelectedBusinessId = businessId.Value;

            return View(viewModel);
        }

        private async Task<List<MonthlyData>> GetMonthlyRevenue(int businessId)
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            var monthlyData = await _context.Orders
                .Where(o => o.BusinessId == businessId
                       && !o.IsDeleted
                       && o.status
                       && o.datetime >= sixMonthsAgo)
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

        private async Task<List<MonthlyData>> GetMonthlyExpenses(int businessId)
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            var monthlyData = await _context.Expenses
                .Where(e => e.BusinessId == businessId
                       && !e.IsDeleted
                       && e.ExpenseDate >= sixMonthsAgo)
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

        private async Task<List<TopSellingItem>> GetTopSellingItems(int businessId)
        {
            var topItems = await _context.OrderItems
                .Include(oi => oi.Item)
                .Include(oi => oi.order)
                .Where(oi => oi.order.BusinessId == businessId
                        && !oi.order.IsDeleted
                        && oi.order.status)
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