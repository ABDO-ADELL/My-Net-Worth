using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using PRISM.Models;
using PRISM.Models.Authmodels;

namespace PRISM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchController : ControllerBase
    {
        private readonly AppDbContext _context;


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Branch>>> GetBranches()
        {
            return await _context.Branches.Include(b => b.Business).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Branch>> GetBranch(int id)
        {
            var branch = await _context.Branches
                .Include(b => b.Business)
                .FirstOrDefaultAsync(b => b.BranchId == id);

            if (branch == null)
                return NotFound();

            return branch;
        }

        [HttpPost]
        public async Task<ActionResult<Branch>> CreateBranch(Branch branch)
        {
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBranch), new { id = branch.BranchId }, branch);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBranch(int id, Branch branch)
        {
            if (id != branch.BranchId)
                return BadRequest();

            _context.Entry(branch).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch != null)
            {
                // Soft delete
                branch.IsDeleted = true;
                _context.Update(branch);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }
    }
}