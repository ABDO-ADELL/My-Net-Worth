using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PRISM.Models;
using PRISM.Services;
using System.Security.Claims;

namespace PRISM.Controllers
{
    [Authorize]
    public class BranchController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ViewBagPopulationService _viewBagService;

        public BranchController(AppDbContext context, ViewBagPopulationService viewBagService) : base(context)
        {
            _context = context;
            _viewBagService = viewBagService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var branches = await _context.Branches
                .Include(b => b.Business)
                .Where(b => !b.IsDeleted && b.Business.UserId == userId)
                .ToListAsync();
            
            await this.PopulateBranchesAsync(_viewBagService);
            return View(branches);
        }

        #region Details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();

            var branch = await _context.Branches
                .Include(b => b.Business)
                .Include(b => b.Items)
                .Include(b => b.Inventories)
                .FirstOrDefaultAsync(b => b.BranchId == id
                    && !b.IsDeleted
                    && b.Business.UserId == userId);

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
            await this.PopulateBusinessesAsync(_viewBagService);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Branch branch)
        {
            var userId = GetCurrentUserId();

            ModelState.Remove("Business");
            ModelState.Remove("Items");
            ModelState.Remove("Inventories");

            if (!ModelState.IsValid)
            {
                await this.PopulateBusinessesAsync(_viewBagService);


                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return View(branch);
            }

            try
            {
                // Verify business belongs to current user
                var businessExists = await _context.Businesses
                    .AnyAsync(b => b.BusinessId == branch.BusinessId
                        && !b.IsDeleted
                        && b.UserId == userId);

                if (!businessExists)
                {
                    ModelState.AddModelError("BusinessId", "Selected business does not exist.");
                    ViewBag.Businesses = new SelectList(
                        await _context.Businesses
                            .Where(b => !b.IsDeleted && b.UserId == userId)
                            .ToListAsync(),
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
                    await _context.Businesses
                        .Where(b => !b.IsDeleted && b.UserId == userId)
                        .ToListAsync(),
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
            var userId = GetCurrentUserId();
            var branch = await _context.Branches
                .Include(b => b.Business)
                .FirstOrDefaultAsync(b => b.BranchId == id
                    && !b.IsDeleted
                    && b.Business.UserId == userId);

            if (branch == null)
            {
                return NotFound();
            }

            // Populate businesses dropdown for reassignment
            await this.PopulateBusinessesAsync(_viewBagService);

            return View(branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Branch branch)
        {
            var userId = GetCurrentUserId();

            ModelState.Remove("Business");
            ModelState.Remove("Items");
            ModelState.Remove("Inventories");

            if (!ModelState.IsValid)
            {
                await this.PopulateBusinessesAsync(_viewBagService);
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return View(branch);
            }

            try
            {
                // Load existing branch with security check
                var existingBranch = await _context.Branches
                    .Include(b => b.Business)
                    .FirstOrDefaultAsync(b => b.BranchId == branch.BranchId
                        && !b.IsDeleted
                        && b.Business.UserId == userId);

                if (existingBranch == null)
                {
                    TempData["ErrorMessage"] = "Branch not found or access denied.";
                    return NotFound();
                }

                // Validate the new BusinessId belongs to the current user
                var validBusiness = await _context.Businesses
                    .AnyAsync(b => b.BusinessId == branch.BusinessId
                        && !b.IsDeleted
                        && b.UserId == userId);

                if (!validBusiness)
                {
                    ModelState.AddModelError("BusinessId", "Selected business is invalid or does not belong to you.");
                    await this.PopulateBusinessesAsync(_viewBagService);
                    TempData["ErrorMessage"] = "You can only assign branches to your own businesses.";
                    return View(branch);
                }

                // Update fields - user can reassign to a different business they own
                existingBranch.Name = branch.Name;
                existingBranch.Address = branch.Address;
                existingBranch.Phone = branch.Phone;
                existingBranch.BusinessId = branch.BusinessId; // Allow reassignment

                _context.Branches.Update(existingBranch);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Branch '{branch.Name}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BranchExists(branch.BranchId))
                {
                    TempData["ErrorMessage"] = "Branch no longer exists.";
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating branch: {ex.Message}");
                ModelState.AddModelError("", "An unexpected error occurred while updating the branch.");
                await this.PopulateBusinessesAsync(_viewBagService);
                TempData["ErrorMessage"] = "An error occurred while updating the branch.";
                return View(branch);
            }
        }
        #endregion

        #region Archive
        [HttpGet]
        public async Task<IActionResult> Archived()
        {
            var userId = GetCurrentUserId();

            var archivedBranches = await _context.Branches
                .Include(b => b.Business)
                .Include(b => b.Items)
                .Include(b => b.Inventories)
                .Where(b => b.IsDeleted && b.Business.UserId == userId)
                .ToListAsync();

            return View(archivedBranches);
        }
        public async Task<IActionResult> Unarchive(int id)
        {
            var Branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.BranchId == id);

            if (Branch == null)
                return NotFound();

            Branch.IsDeleted = false;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Business restored successfully.";

            return RedirectToAction("Archived");
        }

        #endregion

        #region Delete
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();

            var branch = await _context.Branches
                .Include(b => b.Business)
                .FirstOrDefaultAsync(b => b.BranchId == id
                    && b.Business.UserId == userId);

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
            var userId = GetCurrentUserId();

            var branch = await _context.Branches
                .Include(b => b.Business)
                .FirstOrDefaultAsync(b => b.BranchId == id
                    && b.Business.UserId == userId);

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
            var userId = GetCurrentUserId();
            return await _context.Branches
                .Include(b => b.Business)
                .AnyAsync(b => b.BranchId == id && b.Business.UserId == userId);
        }
    }
}