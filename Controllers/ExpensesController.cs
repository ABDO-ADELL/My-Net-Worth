using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PRISM.Models;

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
        public async Task<IActionResult> Index(int businessId, int page = 1, int pageSize = 20,
            DateTime? startDate = null, DateTime? endDate = null, int? branchId = null, int? categoryId = null)
        {
            if (businessId == 0)
                return BadRequest("Business ID is required");

            var expenses = await _expenseBL.GetByBusinessAsync(businessId, page, pageSize, startDate, endDate, branchId, categoryId);

            ViewBag.BusinessId = businessId;
            ViewBag.Page = page;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.BranchId = branchId;
            ViewBag.CategoryId = categoryId;

            // For filters
            ViewBag.Branches = new SelectList(await _context.Branches.Where(b => b.BusinessId == businessId).ToListAsync(), "BranchId", "Name");
            ViewBag.Categories = new SelectList(await _context.ExpenseCategories.Where(c => c.BusinessId == businessId).ToListAsync(), "ExpenseCategoryId", "Name");

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
        public async Task<IActionResult> Create(int businessId)
        {
            if (businessId == 0)
                return BadRequest("Business ID is required");

            ViewBag.BusinessId = businessId;
            ViewBag.Branches = new SelectList(await _context.Branches.Where(b => b.BusinessId == businessId).ToListAsync(), "BranchId", "Name");
            ViewBag.Categories = new SelectList(await _context.ItemCategories.Where(c => c.BusinessId == businessId).ToListAsync(), "ExpenseCategoryId", "Name");

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

            ViewBag.BusinessId = expense.BusinessId;
            ViewBag.Branches = new SelectList(await _context.Branches.Where(b => b.BusinessId == expense.BusinessId).ToListAsync(), "BranchId", "Name", expense.BranchId);
            ViewBag.Categories = new SelectList(await _context.ExpenseCategories.Where(c => c.BusinessId == expense.BusinessId).ToListAsync(), "ExpenseCategoryId", "Name", expense.CategoryId);

            return View(expense);
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var expense = await _expenseBL.GetByIdAsync(id);
            if (expense == null)
                return NotFound();

            ViewBag.Branches = new SelectList(await _context.Branches.Where(b => b.BusinessId == expense.BusinessId).ToListAsync(), "BranchId", "Name", expense.BranchId);
            ViewBag.Categories = new SelectList(await _context.ExpenseCategories.Where(c => c.BusinessId == expense.BusinessId).ToListAsync(), "ExpenseCategoryId", "Name", expense.CategoryId);

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

            ViewBag.Branches = new SelectList(await _context.Branches.Where(b => b.BusinessId == expense.BusinessId).ToListAsync(), "BranchId", "Name", expense.BranchId);
            ViewBag.Categories = new SelectList(await _context.ExpenseCategories.Where(c => c.BusinessId == expense.BusinessId).ToListAsync(), "ExpenseCategoryId", "Name", expense.CategoryId);

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
    }
}