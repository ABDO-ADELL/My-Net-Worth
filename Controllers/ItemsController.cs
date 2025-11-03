using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PRISM.Models;
using System.Linq;
using System.Threading.Tasks;
using PRISM.Models;

namespace PRISM.Controllers
{
    [AllowAnonymous]
    public class ItemsController : Controller
    {
        private readonly AppDbContext _context;

        public ItemsController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET: Items (Index)
        public async Task<IActionResult> Index()
        {
            var items = await _context.Items
                .Include(i => i.ItemCategory)
                .Include(i => i.Branch)
                .Where(i => !i.IsDeleted)
                .ToListAsync();
            return View(items);
        }

        // ✅ GET: Archived Items
        [HttpGet]
        public async Task<IActionResult> Archived()
        {
            var archived = await _context.Items
                .Include(i => i.ItemCategory)
                .Include(i => i.Branch)
                .Where(i => i.IsDeleted)
                .ToListAsync();
            return View(archived);
        }

        // ✅ GET: Search
        [HttpGet]
        public async Task<IActionResult> Search(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return View(new List<Items>());

            var results = await _context.Items
                .Include(i => i.ItemCategory)
                .Include(i => i.Branch)
                .Where(i => i.Name.Contains(query) || i.Sku.Contains(query))
                .ToListAsync();

            return View(results);
        }

        // ✅ GET: Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        // ✅ POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Items item)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(item);
                return View(item);
            }

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ✅ GET: Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            await PopulateDropdowns(item);
            return View(item);
        }

        // ✅ POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Items item)
        {
            if (id != item.ItemId) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(item);
                return View(item);
            }

            try
            {
                _context.Update(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Item updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Items.Any(e => e.ItemId == id))
                    return NotFound();
                throw;
            }
        }

        // ✅ GET: Delete (Confirmation)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Items
                .Include(i => i.ItemCategory)
                .Include(i => i.Branch)
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null) return NotFound();

            return View(item);
        }

        // ✅ POST: Delete (Soft Delete)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                item.IsDeleted = true;
                _context.Items.Update(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Item deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.Items
                .Include(i => i.ItemCategory)
                .Include(i => i.Branch)
                .FirstOrDefaultAsync(c => c.ItemId == id);

            if (item == null)
                return NotFound();

            return View(item);
        }


        // ✅ Helper: Populate Dropdowns
        private async Task PopulateDropdowns(Items? item = null)
        {
            ViewBag.Categories = new SelectList(await _context.ItemCategories.Where(c => !c.IsArchived).ToListAsync(), "CategoryId", "Name", item?.CategoryId);
            ViewBag.Branches = new SelectList(await _context.Branches.Where(b => !b.IsDeleted).ToListAsync(), "BranchId", "Name", item?.BranchId);
            ViewBag.Businesses = new SelectList(await _context.Businesses.ToListAsync(), "BusinessId", "Name", item?.BusinessId);
        }
    }
}
