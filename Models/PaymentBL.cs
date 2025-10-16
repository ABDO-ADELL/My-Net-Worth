using Microsoft.EntityFrameworkCore;

namespace PRISM.Models
{
    public class PaymentBL
    {
        private readonly AppDbContext _context;

        public PaymentBL(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> AddAsync(Payment payment)
        {
            payment.datetime = DateTime.UtcNow; // Fix: Corrected property name to match the Payment class definition  
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<IEnumerable<Payment>> GetByBusinessAsync(int businessId,
            int page = 1, int pageSize = 20, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Payments
                .Include(p => p.Order)
                .Where(p => !p.IsDeleted && p.Order.business_id == businessId);

            if (startDate.HasValue)
                query = query.Where(p => p.datetime >= startDate.Value); // Fix: Corrected property name to match the Payment class definition  

            if (endDate.HasValue)
                query = query.Where(p => p.datetime <= endDate.Value); // Fix: Corrected property name to match the Payment class definition  

            return await query
                .OrderByDescending(p => p.datetime) // Fix: Corrected property name to match the Payment class definition  
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.PaymentId == id && !p.IsDeleted);
        }

        public async Task<bool> UpdateAsync(int id, Payment updated)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == id && !p.IsDeleted);
            if (payment == null) return false;

            payment.Method = updated.Method;
            payment.Amount = updated.Amount;
            payment.OrderId = updated.OrderId;
            payment.datetime = DateTime.UtcNow; // Fix: Corrected property name to match the Payment class definition  

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null || payment.IsDeleted) return false;

            payment.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
