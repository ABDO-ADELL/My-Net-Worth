using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PRISM.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRISM.Controllers
{
    [AllowAnonymous]
    public class SuppliersController : Controller
    {
        private readonly AppDbContext _context;

        public SuppliersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.Suppliers
                .Include(s => s.SupplierItems)
                .ThenInclude(si => si.Item)
                .Where(s => !s.IsDeleted)
                .ToListAsync();
            return View(suppliers);
        }

        // GET: Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateItemsDropdown();
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            // إزالة Validation للـ Navigation Properties
            ModelState.Remove("SupplierItems");
            if (supplier.SupplierItems != null)
            {
                for (int i = 0; i < supplier.SupplierItems.Count; i++)
                {
                    ModelState.Remove($"SupplierItems[{i}].Supplier");
                    ModelState.Remove($"SupplierItems[{i}].Item");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateItemsDropdown();
                return View(supplier);
            }

            // فقط إضافة المورد بما في ذلك SupplierItems
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Supplier added successfully!";
            return RedirectToAction(nameof(Index));
        }


        // GET: Details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.SupplierItems)
                .ThenInclude(si => si.Item)
                .FirstOrDefaultAsync(s => s.SupplierId == id && !s.IsDeleted);

            if (supplier == null)
                return NotFound();

            return View(supplier);
        }

        // GET: Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.SupplierItems)
                .ThenInclude(si => si.Item)
                .FirstOrDefaultAsync(s => s.SupplierId == id && !s.IsDeleted);

            if (supplier == null)
                return NotFound();

            await PopulateItemsDropdown();
            return View(supplier);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            if (id != supplier.SupplierId)
                return NotFound();

            // إزالة Validation للـ Navigation Properties
            ModelState.Remove("SupplierItems");
            if (supplier.SupplierItems != null)
            {
                for (int i = 0; i < supplier.SupplierItems.Count; i++)
                {
                    ModelState.Remove($"SupplierItems[{i}].Supplier");
                    ModelState.Remove($"SupplierItems[{i}].Item");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateItemsDropdown();
                return View(supplier);
            }

            try
            {
                var existingSupplier = await _context.Suppliers
                    .Include(s => s.SupplierItems)
                    .FirstOrDefaultAsync(s => s.SupplierId == id);

                if (existingSupplier == null)
                    return NotFound();

                // تحديث البيانات الأساسية
                existingSupplier.Name = supplier.Name;
                existingSupplier.Email = supplier.Email;
                existingSupplier.Phone = supplier.Phone;

                // حذف كل العناصر القديمة
                _context.SupplierItems.RemoveRange(existingSupplier.SupplierItems);
                await _context.SaveChangesAsync();

                // إضافة العناصر الجديدة بشكل آمن
                if (supplier.SupplierItems != null && supplier.SupplierItems.Any())
                {
                    var itemsToAdd = new List<SupplierItem>();

                    foreach (var item in supplier.SupplierItems)
                    {
                        if (item.ItemId > 0 && !string.IsNullOrEmpty(item.PaymentMethod))
                        {
                            var newSupplierItem = new SupplierItem
                            {
                                SupplierId = id,
                                ItemId = item.ItemId,
                                PurchasePrice = item.PurchasePrice,
                                PaymentMethod = item.PaymentMethod
                            };
                            itemsToAdd.Add(newSupplierItem);
                        }
                    }

                    if (itemsToAdd.Any())
                    {
                        _context.SupplierItems.AddRange(itemsToAdd);
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["SuccessMessage"] = "Supplier updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                await PopulateItemsDropdown();
                return View(supplier);
            }
        }

        // GET: Delete (Soft Delete)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.SupplierItems)
                .ThenInclude(si => si.Item)
                .FirstOrDefaultAsync(s => s.SupplierId == id && !s.IsDeleted);

            if (supplier == null)
                return NotFound();

            return View(supplier);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.SupplierItems)
                .ThenInclude(si => si.Item)
                .FirstOrDefaultAsync(s => s.SupplierId == id && !s.IsDeleted);

            if (supplier != null)
            {
                supplier.IsDeleted = true;
                _context.Suppliers.Update(supplier);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Supplier deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper: Populate Items Dropdown
        private async Task PopulateItemsDropdown()
        {
            var items = await _context.Items
                .Where(i => !i.IsDeleted)
                .Select(i => new SelectListItem
                {
                    Value = i.ItemId.ToString(),
                    Text = i.Name
                })
                .ToListAsync();

            ViewBag.Items = items;
        }

        // GET: Archived Suppliers
        public async Task<IActionResult> Archived()
        {
            var archivedSuppliers = await _context.Suppliers
                .Where(s => s.IsDeleted)
                .Include(s => s.SupplierItems)   // << مهم جداً
                .ToListAsync();

            return View(archivedSuppliers);
        }

    }
}
