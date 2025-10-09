using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRISM.Models;

namespace PRISM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Owner,Accountant")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentBL _paymentBL;

        public PaymentsController(AppDbContext context)
        {
            _paymentBL = new PaymentBL(context);
        }

        // POST /api/payments
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] Payment payment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _paymentBL.AddAsync(payment);
            return CreatedAtAction(nameof(GetPaymentById), new { id = result.PaymentId }, result);
        }

        // GET /api/payments/{businessId}?page=1&pageSize=20&startDate=2025-01-01&endDate=2025-01-31
        [HttpGet("{businessId}")]
        public async Task<IActionResult> GetPaymentsByBusiness(
            int businessId,
            int page = 1,
            int pageSize = 20,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var payments = await _paymentBL.GetByBusinessAsync(businessId, page, pageSize, startDate, endDate);
            return Ok(payments);
        }

        // GET /api/payments/details/{id}
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            var payment = await _paymentBL.GetByIdAsync(id);
            if (payment == null) return NotFound("Payment not found");
            return Ok(payment);
        }

        // PUT /api/payments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] Payment payment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _paymentBL.UpdateAsync(id, payment);
            if (!success) return NotFound("Payment not found or deleted");

            return NoContent();
        }

        // DELETE /api/payments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var success = await _paymentBL.SoftDeleteAsync(id);
            if (!success) return NotFound("Payment not found or already deleted");

            return Ok("Payment archived successfully");
        }
    }
}
