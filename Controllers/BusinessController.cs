using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PRISM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly AppDbContext _context;


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Business>>> GetBusinesses()
        {
            return await _context.Businesses.Include(b => b.Branches).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Business>> GetBusiness(int id)
        {
            var business = await _context.Businesses
                .Include(b => b.Branches)
                .FirstOrDefaultAsync(b => b.BusinessId == id);

            if (business == null)
                return NotFound();

            return business;
        }

        [HttpPost]
        public async Task<ActionResult<Business>> CreateBusiness(Business business)
        {
            _context.Businesses.Add(business);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBusiness), new { id = business.BusinessId }, business);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBusiness(int id, Business business)
        {
            if (id != business.BusinessId)
                return BadRequest();

            _context.Entry(business).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBusiness(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business != null)
            {
                // Soft delete
                business.IsDeleted = true;
                _context.Update(business);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }
    }
}
