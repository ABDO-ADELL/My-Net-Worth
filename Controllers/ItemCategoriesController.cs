using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRISM.Models;

namespace PRISM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin,Owner,Accountant")]
    [AllowAnonymous]
    public class ItemCategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItemCategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ POST /api/itemcategories → Add new category
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] ItemCategory category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ✅ Check if BusinessId exists
            var businessExists = await _context.Businesses.AnyAsync(b => b.BusinessId == category.BusinessId);
            if (!businessExists)
                return BadRequest(new { message = "Invalid BusinessId" });

            // ✅ Check if category name already exists for the same business
            var exists = await _context.ItemCategories
                .AnyAsync(c => c.BusinessId == category.BusinessId &&
                               c.Name.ToLower() == category.Name.ToLower() &&
                               !c.IsArchived);

            if (exists)
                return Conflict(new { message = "Category name already exists for this business" });

            _context.ItemCategories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category created successfully", category });
        }

        // ✅ GET /api/itemcategories/{businessId} → Get categories of a business
        [HttpGet("{businessId}")]
        public async Task<IActionResult> GetCategories(int businessId)
        {
            var businessExists = await _context.Businesses.AnyAsync(b => b.BusinessId == businessId);
            if (!businessExists)
                return BadRequest(new { message = "Invalid BusinessId" });

            var categories = await _context.ItemCategories
                .Where(c => c.BusinessId == businessId && !c.IsArchived)
                .Include(c => c.Items)
                .ToListAsync();

            if (categories.Count == 0)
                return NotFound(new { message = "No categories found for this business" });

            return Ok(categories);
        }

        // ✅ PUT /api/itemcategories/{id} → Update category
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] ItemCategory updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _context.ItemCategories.FindAsync(id);
            if (category == null)
                return NotFound(new { message = "Category not found" });

            // ✅ Check duplicate name within same business
            var duplicate = await _context.ItemCategories
                .AnyAsync(c => c.BusinessId == category.BusinessId &&
                               c.Name.ToLower() == updated.Name.ToLower() &&
                               c.CategoryId != id &&
                               !c.IsArchived);

            if (duplicate)
                return Conflict(new { message = "Category name already exists" });

            category.Name = updated.Name.Trim();
            _context.ItemCategories.Update(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category updated successfully", category });
        }

        // ✅ DELETE /api/itemcategories/{id} → Archive category
        [HttpDelete("{id}")]
        public async Task<IActionResult> ArchiveCategory(int id)
        {
            var category = await _context.ItemCategories.FindAsync(id);
            if (category == null)
                return NotFound(new { message = "Category not found" });

            if (category.IsArchived)
                return BadRequest(new { message = "Category is already archived" });

            category.IsArchived = true;
            _context.ItemCategories.Update(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category archived successfully" });
        }
    }
}
