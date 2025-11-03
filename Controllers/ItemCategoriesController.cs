using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PRISM.Models;

namespace PRISM.Controllers
{
    [AllowAnonymous]
    public class ItemCategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public ItemCategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET: Create (عرض صفحة إضافة كاتيجوري)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // بنبعت لليو ViewBag فيه أسماء الـ Businesses كلها
            ViewBag.Businesses = new SelectList(await _context.Businesses.ToListAsync(), "BusinessId", "Name");
            return View();
        }

        // ✅ POST: Create (حفظ البيانات)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemCategory category)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Businesses = new SelectList(await _context.Businesses.ToListAsync(), "BusinessId", "Name");
                return View(category);
            }

            // تأكد أن الـ BusinessId صحيح
            var businessExists = await _context.Businesses.AnyAsync(b => b.BusinessId == category.BusinessId);
            if (!businessExists)
            {
                ModelState.AddModelError("BusinessId", "Please select a valid Business.");
                ViewBag.Businesses = new SelectList(await _context.Businesses.ToListAsync(), "BusinessId", "Name");
                return View(category);
            }

            _context.ItemCategories.Add(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Category added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ✅ عرض كل الكاتيجوريز
        public async Task<IActionResult> Index()
        {
            var categories = await _context.ItemCategories
                .Include(c => c.Business)
                .Where(c => !c.IsArchived)
                .ToListAsync();
            return View(categories);
        }

        // ✅ عرض المؤرشفين
        [HttpGet]
        public async Task<IActionResult> Archived()
        {
            var archived = await _context.ItemCategories
                .Include(c => c.Business)
                .Where(c => c.IsArchived)
                .ToListAsync();

            return View(archived);
        }

        // ✅ البحث
        [HttpGet]
        public async Task<IActionResult> Search(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                // لو البحث فاضي، رجّعي الصفحة فاضية أو كل الكاتيجوريز حسب رغبتك
                return View(new List<ItemCategory>());
            }

            var results = await _context.ItemCategories
                .Include(c => c.Business)
                .Where(c => c.Name.Contains(query))
                .ToListAsync();

            return View(results);
        }

        // ✅ GET: Edit (عرض صفحة التعديل)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.ItemCategories.FindAsync(id);
            if (category == null)
                return NotFound();

            // بنبعت ليست البزنس عشان تظهر في الدروب داون
            ViewBag.Businesses = new SelectList(await _context.Businesses.ToListAsync(), "BusinessId", "Name", category.BusinessId);

            return View(category);
        }

        // ✅ POST: Edit (تعديل الكاتيجوري)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemCategory category)
        {
            if (id != category.CategoryId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Businesses = new SelectList(await _context.Businesses.ToListAsync(), "BusinessId", "Name", category.BusinessId);
                return View(category);
            }

            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ItemCategories.Any(e => e.CategoryId == id))
                    return NotFound();
                throw;
            }
        }
        // ✅ GET: Details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.ItemCategories
                .Include(c => c.Business)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // ✅ GET: Delete (عرض صفحة تأكيد الحذف)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.ItemCategories
                .Include(c => c.Business)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // ✅ POST: Delete (تنفيذ الحذف فعليًا)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.ItemCategories.FindAsync(id);
            if (category == null)
                return NotFound();

            // لو عايزة تمسحيه نهائيًا:
            //_context.ItemCategories.Remove(category);

            // أو لو عايزة تعمليه Archive بدل حذف:
            category.IsArchived = true;
            _context.ItemCategories.Update(category);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }


    }
}
