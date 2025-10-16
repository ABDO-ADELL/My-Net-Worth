using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRISM;
using PRISM.Models;

namespace PRISM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItemsController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ POST /api/items → Add new item
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] Item item)
        {
            // ✅ Basic model validation
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input data", errors = ModelState });

            // ✅ Check foreign keys
            if (!await _context.Branches.AnyAsync(b => b.BranchId == item.BranchId))
                return BadRequest(new { message = $"Branch with ID {item.BranchId} not found." });

            if (!await _context.ItemCategories.AnyAsync(c => c.CategoryId == item.CategoryId))
                return BadRequest(new { message = $"Category with ID {item.CategoryId} not found." });

            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == item.BusinessId))
                return BadRequest(new { message = $"Business with ID {item.BusinessId} not found." });

            try
            {
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Item created successfully", item });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error while saving item", error = ex.Message });
            }
        }

        // ✅ GET /api/items/{businessId}?type=product&page=1&pageSize=10
        [HttpGet("{businessId}")]
        public async Task<IActionResult> GetItems(int businessId, string? type = null, int page = 1, int pageSize = 10)
        {
            var businessExists = await _context.Businesses.AnyAsync(b => b.BusinessId == businessId);
            if (!businessExists)
                return NotFound(new { message = $"Business with ID {businessId} not found." });

            var query = _context.Items
                .Where(i => i.BusinessId == businessId && !i.IsDeleted)
                .Include(i => i.ItemCategory)
                .AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(i => i.ItemCategory.Name.ToLower().Contains(type.ToLower()));

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { totalCount, currentPage = page, pageSize, data = items });
        }

        // ✅ GET /api/items/details/{id} → Get item details
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await _context.Items
                .Include(i => i.ItemCategory)
                .Include(i => i.Branch)
                .FirstOrDefaultAsync(i => i.ItemId == id && !i.IsDeleted);

            if (item == null)
                return NotFound(new { message = $"Item with ID {id} not found." });

            return Ok(item);
        }

        // ✅ PUT /api/items/{id} → Update item
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] Item updatedItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input data", errors = ModelState });

            var item = await _context.Items.FindAsync(id);
            if (item == null || item.IsDeleted)
                return NotFound(new { message = $"Item with ID {id} not found." });

            // ✅ Validate foreign keys before saving
            if (!await _context.Branches.AnyAsync(b => b.BranchId == updatedItem.BranchId))
                return BadRequest(new { message = $"Branch with ID {updatedItem.BranchId} not found." });

            if (!await _context.ItemCategories.AnyAsync(c => c.CategoryId == updatedItem.CategoryId))
                return BadRequest(new { message = $"Category with ID {updatedItem.CategoryId} not found." });

            try
            {
                item.Name = updatedItem.Name;
                item.Sku = updatedItem.Sku;
                item.CostPrice = updatedItem.CostPrice;
                item.SellPrice = updatedItem.SellPrice;
                item.Description = updatedItem.Description;
                item.CategoryId = updatedItem.CategoryId;
                item.DurationMinutes = updatedItem.DurationMinutes;
                item.BranchId = updatedItem.BranchId;

                _context.Items.Update(item);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Item updated successfully", item });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error while updating item", error = ex.Message });
            }
        }

        // ✅ DELETE /api/items/{id} → Soft delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> ArchiveItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return NotFound(new { message = $"Item with ID {id} not found." });

            try
            {
                item.IsDeleted = true;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Item archived successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error while archiving item", error = ex.Message });
            }
        }
    }
}
