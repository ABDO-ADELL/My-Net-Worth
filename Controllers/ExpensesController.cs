using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRISM.Models;

namespace PRISM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Owner,Accountant")]
    public class ExpensesController : ControllerBase
    {
        private readonly ExpenseBL _expenseBL;

        public ExpensesController(AppDbContext context)
        {
            _expenseBL = new ExpenseBL(context);
        }

        // POST /api/expenses
        [HttpPost]
        public async Task<IActionResult> CreateExpense([FromBody] Expense expense)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _expenseBL.AddAsync(expense);
            return CreatedAtAction(nameof(GetExpenseById), new { id = result.ExpenseId }, result);
        }

        // GET /api/expenses/{businessId}?page=1&pageSize=20&branchId=2&categoryId=3
        [HttpGet("{businessId}")]
        public async Task<IActionResult> GetExpensesByBusiness(
            int businessId,
            int page = 1,
            int pageSize = 20,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? branchId = null,
            int? categoryId = null)
        {
            var expenses = await _expenseBL.GetByBusinessAsync(businessId, page, pageSize, startDate, endDate, branchId, categoryId);
            return Ok(expenses);
        }

        // GET /api/expenses/details/{id}
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetExpenseById(int id)
        {
            var expense = await _expenseBL.GetByIdAsync(id);
            if (expense == null) return NotFound("Expense not found");
            return Ok(expense);
        }

        // PUT /api/expenses/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(int id, [FromBody] Expense expense)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _expenseBL.UpdateAsync(id, expense);
            if (!success) return NotFound("Expense not found or deleted");

            return NoContent();
        }

        // DELETE /api/expenses/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var success = await _expenseBL.SoftDeleteAsync(id);
            if (!success) return NotFound("Expense not found or already deleted");

            return Ok("Expense archived successfully");
        }
    }
}
