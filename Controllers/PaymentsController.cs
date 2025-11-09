using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PRISM.Models;

namespace PRISM.Controllers
{
    //[Authorize(Roles = "Admin,Owner,Accountant")]
    public class PaymentsController : Controller
    {
        private readonly PaymentBL _paymentBL;
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _paymentBL = new PaymentBL(context);
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index(int businessId, int page = 1, int pageSize = 20,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            if (businessId == 0)
                return BadRequest("Business ID is required");

            var payments = await _paymentBL.GetByBusinessAsync(businessId, page, pageSize, startDate, endDate);

            ViewBag.BusinessId = businessId;
            ViewBag.Page = page;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(payments);
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentBL.GetByIdAsync(id);
            if (payment == null)
                return NotFound();

            return View(payment);
        }

        // GET: Payments/Create
        public async Task<IActionResult> Create(int businessId)
        {
            if (businessId == 0)
                return BadRequest("Business ID is required");

            ViewBag.BusinessId = businessId;

            ViewBag.Orders = new SelectList(await _context.Orders.Where(o => o.BusinessId == businessId).ToListAsync(), "OrderId", "OrderId");
            return View();
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment, int businessId)
        {
            if (ModelState.IsValid)
            {
                await _paymentBL.AddAsync(payment);
                return RedirectToAction(nameof(Index), new { businessId });
            }

            ViewBag.BusinessId = businessId;
            ViewBag.Orders = new SelectList(await _context.Orders.Where(o => o.BusinessId == businessId).ToListAsync(), "OrderId", "OrderId", payment.OrderId);

            return View(payment);
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var payment = await _paymentBL.GetByIdAsync(id);
            if (payment == null)
                return NotFound();

            ViewBag.Orders = new SelectList(await _context.Orders.Where(o => o.BusinessId == payment.Order.BusinessId).ToListAsync(), "OrderId", "OrderId", payment.OrderId);
            return View(payment);
        }

        // POST: Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Payment payment)
        {
            if (id != payment.PaymentId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var success = await _paymentBL.UpdateAsync(id, payment);
                if (!success)
                    return NotFound();

                return RedirectToAction(nameof(Index), new { businessId = payment.Order.BusinessId });
            }

            ViewBag.Orders = new SelectList(await _context.Orders.Where(o => o.BusinessId == payment.Order.BusinessId).ToListAsync(), "OrderId", "OrderId", payment.OrderId);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _paymentBL.GetByIdAsync(id);
            if (payment == null)
                return NotFound();

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _paymentBL.GetByIdAsync(id);
            if (payment == null)
                return NotFound();

            var success = await _paymentBL.SoftDeleteAsync(id);
            if (!success)
                return NotFound();

            return RedirectToAction(nameof(Index), new { businessId = payment.Order.BusinessId });
        }
    }
}