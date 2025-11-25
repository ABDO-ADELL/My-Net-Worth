using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PRISM.Models;
using System.Security.Claims;

namespace PRISM.Controllers
{
    [AllowAnonymous]
    public class ItemCategoriesController : BaseController
    {
        private readonly AppDbContext _context;

        public ItemCategoriesController(AppDbContext context):base(context)
        {
            _context = context;
        }

        // ✅ GET: Create (عرض صفحة إضافة كاتيجوري)
        [HttpGet]
        public async Task<IActionResult> Create()
        {

            await PopulateDropdowns();
            return View();

        }

        // ✅ POST: Create (حفظ البيانات)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemCategory category)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
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
            var userId = GetCurrentUserId();

            var categories = await _context.ItemCategories
                .Include(c => c.Business)
                .Where(c => !c.IsArchived &&c.Business.UserId ==userId)
                .ToListAsync();
            return View(categories);
        }

        // ✅ عرض المؤرشفين
        [HttpGet]
        public async Task<IActionResult> Archived()
        {
            var userId = GetCurrentUserId();


            var archived = await _context.ItemCategories
                .Include(c => c.Business)
                .Where(c => c.IsArchived&&c.Business.UserId==userId)
                .ToListAsync();

            return View(archived);
        }

        // ✅ البحث
        [HttpGet]
        public async Task<IActionResult> Search(string? query)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(query))
            {
                // لو البحث فاضي، رجّعي الصفحة فاضية أو كل الكاتيجوريز حسب رغبتك
                return View(new List<ItemCategory>());
            }

            var results = await _context.ItemCategories
                .Include(c => c.Business)
                .Where(c => c.Name.Contains(query) &&c.Business.UserId==userId)
                .ToListAsync();

            return View(results);
        }

        // ✅ GET: Edit (عرض صفحة التعديل)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();

            var category = await _context.ItemCategories.Where(a=>a.Business.UserId == userId && a.CategoryId==id).FirstOrDefaultAsync();

            if (category == null)
                return NotFound();

            // بنبعت ليست البزنس عشان تظهر في الدروب داون
            ViewBag.Businesses = new SelectList(await _context.Businesses.Where(b=>b.UserId==userId && !b.IsDeleted).ToListAsync(), "BusinessId", "Name", category.BusinessId);

            return View(category);
        }

        // ✅ POST: Edit (تعديل الكاتيجوري)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemCategory category)
        {
            var userId = GetCurrentUserId();
            if (id != category.CategoryId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Businesses = new SelectList(await _context.Businesses.Where(b=>b.UserId==userId).ToListAsync(), "BusinessId", "Name", category.BusinessId);
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
            var userId = GetCurrentUserId();

            var category = await _context.ItemCategories
                .Include(c => c.Business).Where(a=>a.Business.UserId==userId &&a.CategoryId == id)
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound();

            return View(category);
        }
        public async Task<IActionResult> Unarchive(int id)
        {
            var business = await _context.ItemCategories
                .FirstOrDefaultAsync(b => b.CategoryId == id);

            if (business == null)
                return NotFound();

            business.IsArchived = false;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Business restored successfully.";

            return RedirectToAction("Archived");
        }


        // ✅ GET: Delete (عرض صفحة تأكيد الحذف)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();

            var category = await _context.ItemCategories
                .Include(c => c.Business).Where(a => a.Business.UserId == userId && a.CategoryId == id)
                .FirstOrDefaultAsync();

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
        private async Task PopulateDropdowns(Items? item = null)
        {
            var userId = GetCurrentUserId();

            ViewBag.Categories = new SelectList(await _context.ItemCategories.Where(c => !c.IsArchived && c.Business.UserId == userId && c.Business.IsDeleted).ToListAsync(), "CategoryId", "Name", item?.CategoryId);
            ViewBag.Branches = new SelectList(await _context.Branches.Where(b => !b.IsDeleted && b.Business.UserId == userId).ToListAsync(), "BranchId", "Name", item?.BranchId);
            ViewBag.Businesses = new SelectList(await _context.Businesses.Where(a => a.UserId== userId && !a.IsDeleted).ToListAsync(), "BusinessId", "Name", item?.BusinessId);
        }


    }
}
