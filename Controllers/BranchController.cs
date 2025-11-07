using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PRISM.Models;

namespace PRISM.Controllers
{
    public class BranchController : Controller
    {
        private readonly AppDbContext _context;

        public BranchController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var branches = await _context.Branches
                .Include(b => b.Business)
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.Name)
                .ToListAsync();
            return View(branches);
        }

        #region Details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var branch = await _context.Branches
                .Include(b => b.Business)
                .Include(b => b.Items)
                .Include(b => b.Inventories)
                .FirstOrDefaultAsync(b => b.BranchId == id);

            if (branch == null)
            {
                return NotFound();
            }
            return View(branch);
        }
        #endregion

        #region Create  
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Businesses = new SelectList(
                await _context.Businesses.Where(b => !b.IsDeleted).ToListAsync(),
                "BusinessId",
                "Name"
            );
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Branch branch)
        {
            ModelState.Remove("Business");
            ModelState.Remove("Items");
            ModelState.Remove("Inventories");

            if (!ModelState.IsValid)
            {
                ViewBag.Businesses = new SelectList(
                    await _context.Businesses.Where(b => !b.IsDeleted).ToListAsync(),
                    "BusinessId",
                    "Name",
                    branch.BusinessId
                );

                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return View(branch);
            }

            try
            {
                var businessExists = await _context.Businesses
                    .AnyAsync(b => b.BusinessId == branch.BusinessId && !b.IsDeleted);

                if (!businessExists)
                {
                    ModelState.AddModelError("BusinessId", "Selected business does not exist.");
                    ViewBag.Businesses = new SelectList(
                        await _context.Businesses.Where(b => !b.IsDeleted).ToListAsync(),
                        "BusinessId",
                        "Name"
                    );
                    TempData["ErrorMessage"] = "Selected business is invalid.";
                    return View(branch);
                }

                branch.IsDeleted = false;

                _context.Branches.Add(branch);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Branch '{branch.Name}' added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating branch: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                ModelState.AddModelError("", $"Error creating branch: {ex.Message}");
                ViewBag.Businesses = new SelectList(
                    await _context.Businesses.Where(b => !b.IsDeleted).ToListAsync(),
                    "BusinessId",
                    "Name",
                    branch.BusinessId
                );
                TempData["ErrorMessage"] = "An error occurred while creating the branch.";
                return View(branch);
            }
        }
        #endregion

        #region Edit    
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null || branch.IsDeleted)
            {
                return NotFound();
            }

            ViewBag.Businesses = new SelectList(
                await _context.Businesses.Where(b => !b.IsDeleted).ToListAsync(),
                "BusinessId",
                "Name",
                branch.BusinessId
            );

            return View(branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Branch branch)
        {
            if (id != branch.BranchId)
            {
                return NotFound();
            }

            ModelState.Remove("Business");
            ModelState.Remove("Items");
            ModelState.Remove("Inventories");

            if (!ModelState.IsValid)
            {
                ViewBag.Businesses = new SelectList(
                    await _context.Businesses.Where(b => !b.IsDeleted).ToListAsync(),
                    "BusinessId",
                    "Name",
                    branch.BusinessId
                );
                return View(branch);
            }

            try
            {
                _context.Branches.Update(branch);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Branch '{branch.Name}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BranchExists(branch.BranchId))
                {
                    return NotFound();
                }
                throw;
            }
        }
        #endregion

        #region Archive
        [HttpGet]
        public async Task<IActionResult> Archived()
        {
            var archivedBranches = await _context.Branches
                .Include(b => b.Business)
                .Include(b => b.Items)
                .Include(b => b.Inventories)
                .Where(b => b.IsDeleted)
                .ToListAsync();

            return View(archivedBranches);
        }
        #endregion

        #region Delete
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var branch = await _context.Branches
                .Include(b => b.Business)
                .FirstOrDefaultAsync(b => b.BranchId == id);

            if (branch == null)
            {
                return NotFound();
            }

            return View(branch);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound();
            }

            branch.IsDeleted = true;
            _context.Branches.Update(branch);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Branch '{branch.Name}' deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        #endregion

        private async Task<bool> BranchExists(int id)
        {
            return await _context.Branches.AnyAsync(e => e.BranchId == id);
        }
    }
}
