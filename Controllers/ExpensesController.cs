using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PRISM.Models;
using System.Security.Claims;

namespace PRISM.Controllers
{
    //[Authorize(Roles = "Admin,Owner,Accountant")]
    public class ExpensesController : Controller
    {
        private readonly ExpenseBL _expenseBL;
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _expenseBL = new ExpenseBL(context);
            _context = context;
        }

        // GET: Expenses
        public async Task<IActionResult> Index(int? businessId = null, int page = 1, int pageSize = 20,
            DateTime? startDate = null, DateTime? endDate = null, int? branchId = null, string category = null)
        {
            // Get user's businesses
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userBusinesses = await _context.Businesses
                .Where(b => !b.IsDeleted && b.UserId == userId)
                .ToListAsync();

            if (!userBusinesses.Any())
            {
                ViewBag.NoBusiness = true;
                return View(new List<Expense>());
            }

            // If no businessId provided, use the first business or selected one from TempData
            if (!businessId.HasValue || businessId.Value == 0)
            {
                if (TempData["SelectedBusinessId"] != null)
                {
                    businessId = (int)TempData["SelectedBusinessId"];
                    TempData.Keep("SelectedBusinessId");
                }
                else
                {
                    businessId = userBusinesses.First().BusinessId;
                }
            }

            // Get expenses
            var expenses = await _expenseBL.GetByBusinessAsync(businessId.Value, page, pageSize, startDate, endDate, branchId);

            // Filter by category if provided
            if (!string.IsNullOrEmpty(category))
            {
                expenses = expenses.Where(e => e.Category == category).ToList();
            }

            // Prepare ViewBag data
            ViewBag.BusinessId = businessId;
            ViewBag.Page = page;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.BranchId = branchId;
            ViewBag.Category = category;

            // Businesses dropdown
            ViewBag.Businesses = new SelectList(
                userBusinesses,
                "BusinessId",
                "Name",
                businessId
            );

            // Branches dropdown - for selected business
            var branches = await _context.Branches
                .Where(b => b.BusinessId == businessId.Value && !b.IsDeleted)
                .ToListAsync();

            ViewBag.Branches = new SelectList(branches, "BranchId", "Name", branchId);

            // Categories dropdown - get distinct categories from expenses
            var allCategories = await _context.Expenses
                .Where(e => e.BusinessId == businessId.Value && !e.IsDeleted && !string.IsNullOrEmpty(e.Category))
                .Select(e => e.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.Categories = allCategories;

            return View(expenses);
        }

        // GET: Expenses/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var expense = await _expenseBL.GetByIdAsync(id);
            if (expense == null)
                return NotFound();

            return View(expense);
        }

        // GET: Expenses/Create
        public async Task<IActionResult> Create(int? businessId)
        {
            // Get user's businesses
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userBusinesses = await _context.Businesses
                .Where(b => !b.IsDeleted && b.UserId == userId)
                .ToListAsync();

            // If businessId not provided, try to get from session/cookie or use first business
            if (!businessId.HasValue || businessId.Value == 0)
            {
                // Check if there's a selected business in TempData (from dashboard)
                if (TempData["SelectedBusinessId"] != null)
                {
                    businessId = (int)TempData["SelectedBusinessId"];
                    TempData.Keep("SelectedBusinessId"); // Keep for next request
                }
                else if (userBusinesses.Any())
                {
                    businessId = userBusinesses.First().BusinessId;
                }
            }

            // Load all businesses for dropdown
            ViewBag.Businesses = new SelectList(
                userBusinesses,
                "BusinessId",
                "Name",
                businessId
            );

            // If businessId is provided, load branches for that business
            if (businessId.HasValue && businessId.Value > 0)
            {
                ViewBag.Branches = new SelectList(
                    await _context.Branches
                        .Where(b => b.BusinessId == businessId.Value && !b.IsDeleted)
                        .ToListAsync(),
                    "BranchId",
                    "Name"
                );
                ViewBag.SelectedBusinessId = businessId.Value;
            }
            else
            {
                ViewBag.Branches = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.SelectedBusinessId = 0;
            }

            return View();
        }

        // POST: Expenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense)
        {
            if (ModelState.IsValid)
            {
                await _expenseBL.AddAsync(expense);
                return RedirectToAction(nameof(Index), new { businessId = expense.BusinessId });
            }

            // Reload dropdowns on error
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewBag.Businesses = new SelectList(
                await _context.Businesses
                    .Where(b => !b.IsDeleted && b.UserId == userId)
                    .ToListAsync(),
                "BusinessId",
                "Name",
                expense.BusinessId
            );

            ViewBag.Branches = new SelectList(
                await _context.Branches
                    .Where(b => b.BusinessId == expense.BusinessId && !b.IsDeleted)
                    .ToListAsync(),
                "BranchId",
                "Name",
                expense.BranchId
            );

            return View(expense);
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var expense = await _expenseBL.GetByIdAsync(id);
            if (expense == null)
                return NotFound();

            ViewBag.Branches = new SelectList(
                await _context.Branches
                    .Where(b => b.BusinessId == expense.BusinessId && !b.IsDeleted)
                    .ToListAsync(),
                "BranchId",
                "Name",
                expense.BranchId
            );

            return View(expense);
        }

        // POST: Expenses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expense expense)
        {
            if (id != expense.ExpenseId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var success = await _expenseBL.UpdateAsync(id, expense);
                if (!success)
                    return NotFound();

                return RedirectToAction(nameof(Index), new { businessId = expense.BusinessId });
            }

            ViewBag.Branches = new SelectList(
                await _context.Branches
                    .Where(b => b.BusinessId == expense.BusinessId && !b.IsDeleted)
                    .ToListAsync(),
                "BranchId",
                "Name",
                expense.BranchId
            );

            return View(expense);
        }

        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _expenseBL.GetByIdAsync(id);
            if (expense == null)
                return NotFound();

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _expenseBL.GetByIdAsync(id);
            if (expense == null)
                return NotFound();

            var success = await _expenseBL.SoftDeleteAsync(id);
            if (!success)
                return NotFound();

            return RedirectToAction(nameof(Index), new { businessId = expense.BusinessId });
        }

        // API endpoint to get branches by business
        [HttpGet]
        public async Task<JsonResult> GetBranchesByBusiness(int businessId)
        {
            var branches = await _context.Branches
                .Where(b => b.BusinessId == businessId && !b.IsDeleted)
                .Select(b => new { value = b.BranchId, text = b.Name })
                .ToListAsync();

            return Json(branches);
        }
    }
}