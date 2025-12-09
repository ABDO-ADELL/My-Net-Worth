using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PRISM.DataAccess;
using PRISM.Models;
using System.Security.Claims;

namespace PRISM.Services
{
    // Service for populating ViewBags with dropdown data
    public class ViewBagPopulationService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ViewBagPopulationService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        // Populate ViewBag with businesses for current user
        public async Task<SelectList> GetBusinessesSelectListAsync(int? selectedBusinessId = null)
        {
            var userId = GetCurrentUserId();
            var businesses = await _context.Businesses
                .Where(b => !b.IsDeleted && b.UserId == userId)
                .OrderBy(b => b.Name)
                .ToListAsync();

            return new SelectList(businesses, "BusinessId", "Name", selectedBusinessId);
        }

        // Populate ViewBag with branches for current user or specific business
        public async Task<SelectList> GetBranchesSelectListAsync(int? businessId = null, int? selectedBranchId = null)
        {
            var userId = GetCurrentUserId();

            var query = _context.Branches
                .Include(b => b.Business)
                .Where(b => !b.IsDeleted && b.Business.UserId == userId);

            if (businessId.HasValue && businessId.Value > 0)
            {
                query = query.Where(b => b.BusinessId == businessId.Value);
            }

            var branches = await query
                .OrderBy(b => b.Name)
                .Select(b => new
                {
                    b.BranchId,
                    DisplayName = b.Name + " - " + b.Business.Name
                })
                .ToListAsync();

            return new SelectList(branches, "BranchId", "DisplayName", selectedBranchId);
        }

        // Populate ViewBag with customers for current user
        public async Task<SelectList> GetCustomersSelectListAsync(int? selectedCustomerId = null)
        {
            var userId = GetCurrentUserId();
            var customers = await _context.Customers
                .Include(c => c.Branch)
                    .ThenInclude(b => b.Business)
                .Where(c => c.Branch.Business.UserId == userId)
                .OrderBy(c => c.FullName)
                .ToListAsync();

            return new SelectList(customers, "CustomerId", "FullName", selectedCustomerId);
        }

        // Populate ViewBag with item categories for current user or specific business
        public async Task<SelectList> GetCategoriesSelectListAsync(int? businessId = null, int? selectedCategoryId = null)
        {
            var userId = GetCurrentUserId();

            var query = _context.ItemCategories
                .Include(c => c.Business)
                .Where(c => !c.IsArchived && c.Business.UserId == userId);

            if (businessId.HasValue && businessId.Value > 0)
            {
                query = query.Where(c => c.BusinessId == businessId.Value);
            }

            var categories = await query
                .OrderBy(c => c.Name)
                .ToListAsync();

            return new SelectList(categories, "CategoryId", "Name", selectedCategoryId);
        }

        // Populate ViewBag with items for current user, optionally filtered by business or branch
        public async Task<SelectList> GetItemsSelectListAsync(int? businessId = null, int? branchId = null, int? selectedItemId = null)
        {
            var userId = GetCurrentUserId();

            var query = _context.Items
                .Include(i => i.Business)
                .Include(i => i.Branch)
                .Include(i => i.ItemCategory)
                .Where(i => !i.IsDeleted && i.Business.UserId == userId);

            if (businessId.HasValue && businessId.Value > 0)
            {
                query = query.Where(i => i.BusinessId == businessId.Value);
            }

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(i => i.BranchId == branchId.Value);
            }

            var items = await query
                .OrderBy(i => i.Name)
                .Select(i => new
                {
                    i.ItemId,
                    DisplayName = i.Name + " - " + i.Sku + " ($" + i.SellPrice.ToString("N2") + ")"
                })
                .ToListAsync();

            return new SelectList(items, "ItemId", "DisplayName", selectedItemId);
        }

        // Populate ViewBag with suppliers for current user
        public async Task<SelectList> GetSuppliersSelectListAsync(int? selectedSupplierId = null)
        {
            var userId = GetCurrentUserId();
            var suppliers = await _context.Suppliers
                .Include(s => s.Business)
                .Where(s => !s.IsDeleted && s.Business.UserId == userId)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return new SelectList(suppliers, "SupplierId", "Name", selectedSupplierId);
        }

        // Populate all common ViewBags at once
        public async Task PopulateAllViewBagsAsync(Controller controller, int? businessId = null,
            int? branchId = null, int? selectedBusinessId = null, int? selectedBranchId = null,
            int? selectedCustomerId = null, int? selectedCategoryId = null, int? selectedItemId = null)
        {
            controller.ViewBag.Businesses = await GetBusinessesSelectListAsync(selectedBusinessId);
            controller.ViewBag.Branches = await GetBranchesSelectListAsync(businessId, selectedBranchId);
            controller.ViewBag.Customers = await GetCustomersSelectListAsync(selectedCustomerId);
            controller.ViewBag.Categories = await GetCategoriesSelectListAsync(businessId, selectedCategoryId);
            controller.ViewBag.Items = await GetItemsSelectListAsync(businessId, branchId, selectedItemId);
        }
    }

    // Extension methods for Controller to easily populate ViewBags
    public static class ViewBagExtensions
    {
        public static async Task PopulateBusinessesAsync(this Controller controller, ViewBagPopulationService service, int? selectedId = null)
        {
            controller.ViewBag.Businesses = await service.GetBusinessesSelectListAsync(selectedId);
        }

        public static async Task PopulateBranchesAsync(this Controller controller, ViewBagPopulationService service, int? businessId = null, int? selectedId = null)
        {
            controller.ViewBag.Branches = await service.GetBranchesSelectListAsync(businessId, selectedId);
        }

        public static async Task PopulateCustomersAsync(this Controller controller, ViewBagPopulationService service, int? selectedId = null)
        {
            controller.ViewBag.Customers = await service.GetCustomersSelectListAsync(selectedId);
        }

        public static async Task PopulateCategoriesAsync(this Controller controller, ViewBagPopulationService service, int? businessId = null, int? selectedId = null)
        {
            controller.ViewBag.Categories = await service.GetCategoriesSelectListAsync(businessId, selectedId);
        }

        public static async Task PopulateItemsAsync(this Controller controller, ViewBagPopulationService service, int? businessId = null, int? branchId = null, int? selectedId = null)
        {
            controller.ViewBag.Items = await service.GetItemsSelectListAsync(businessId, branchId, selectedId);
        }

        public static async Task PopulateAllAsync(this Controller controller, ViewBagPopulationService service,
            int? businessId = null, int? branchId = null, int? selectedBusinessId = null,
            int? selectedBranchId = null, int? selectedCustomerId = null, int? selectedCategoryId = null, int? selectedItemId = null)
        {
            await service.PopulateAllViewBagsAsync(controller, businessId, branchId, selectedBusinessId,
                selectedBranchId, selectedCustomerId, selectedCategoryId, selectedItemId);
        }
    }
}