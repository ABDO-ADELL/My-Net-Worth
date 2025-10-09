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
            payment.Datetime = DateTime.UtcNow;
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
                query = query.Where(p => p.Datetime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.Datetime <= endDate.Value);

            return await query
                .OrderByDescending(p => p.Datetime)
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
            payment.Datetime = DateTime.UtcNow;

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
