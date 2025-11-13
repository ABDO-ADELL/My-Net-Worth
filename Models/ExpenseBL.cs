using Microsoft.EntityFrameworkCore;

namespace PRISM.Models
{
    public class ExpenseBL
    {
        private readonly AppDbContext _context;

        public ExpenseBL(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Expense> AddAsync(Expense expense)
        {
            expense.ExpenseDate = DateTime.UtcNow;
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            return expense;
        }

        public async Task<IEnumerable<Expense>> GetByBusinessAsync(
            int businessId,
            int page = 1,
            int pageSize = 20,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? branchId = null,
            string? category = null)  // ✅ بقت string
        {
            var query = _context.Expenses
                .Include(e => e.Branch)
                .Where(e => !e.IsDeleted && e.BusinessId == businessId);

            if (branchId.HasValue)
                query = query.Where(e => e.BranchId == branchId.Value);

            // ✅ فلترة حسب الاسم النصي
            if (!string.IsNullOrEmpty(category))
                query = query.Where(e => e.Category != null && e.Category == category);

            if (startDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= endDate.Value);

            return await query
                .OrderByDescending(e => e.ExpenseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Expense?> GetByIdAsync(int id)
        {
            return await _context.Expenses
                .Include(e => e.Branch)
                .FirstOrDefaultAsync(e => e.ExpenseId == id && !e.IsDeleted);
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null || expense.IsDeleted)
                return false;

            expense.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(int id, Expense updated)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.ExpenseId == id && !e.IsDeleted);

            if (expense == null)
                return false;

            expense.Amount = updated.Amount;
            expense.PaymentMethod = updated.PaymentMethod;
            expense.Description = updated.Description;
            expense.Category = updated.Category; // ✅ بقت string
            expense.BranchId = updated.BranchId;
            expense.ExpenseDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
