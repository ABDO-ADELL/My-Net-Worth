using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace PRISM.Controllers
{
    public class BusinessController : BaseController
    {
        private readonly AppDbContext _context;
        public BusinessController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var businesses = _context.Businesses.Where(o=>o.UserId==userId)
                .OrderBy(b => b.Name);
            return View(businesses.ToList());
        }
        [HttpGet]
        public IActionResult Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = _context.Businesses.FirstOrDefault(b => b.BusinessId == id &&b.UserId==userId);
            if (business is null)
            {
                return NotFound();
                //RedirectToAction(SD.NotFoundPage,controllerName: SD.HomeController);
            }
            return View(business);
        }

        #region Create  
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Business business)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            SetUserId(business);

            ModelState.Remove("UserId");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                return View(business);
            }
            _context.Businesses.Add(business);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Edit    
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var business = _context.Businesses.FirstOrDefault(b => b.BusinessId == id);
            if (business is null)
            {
                return NotFound();
                //RedirectToAction(SD.NotFoundPage,controllerName: SD.HomeController);
            }
            return View(business);
        }
        [HttpPost]
        public IActionResult Edit(Business business)
        {
            if (!ModelState.IsValid)
            {
                return View(business);
            }
            _context.Businesses.Update(business);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Archive
        [HttpGet]
        public async Task<IActionResult> Archived()
        {
            var archivedBusinesses = await _context.Businesses
                .Include(b => b.Branches)
                .Include(b => b.Items)
                .Where(b => b.IsDeleted)
                .ToListAsync();

            return View(archivedBusinesses);
        }
        #endregion

        #region Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id) 
        {
            var business = _context.Businesses
                .Include(b => b.Branches)
                .Include(b => b.Items)
                .FirstOrDefault(m => m.BusinessId == id);

            if (business is null)
                return NotFound();

            bool hasActiveRelations =
                 (business.Branches != null && business.Branches.Any(b => !b.IsDeleted)) ||
                 (business.Items != null && business.Items.Any(i => !i.IsDeleted));

            if (hasActiveRelations)
            {
                TempData["ErrorMessage"] = "This business cannot be deleted because it has related records (branches or items).";
                return RedirectToAction(nameof(Index));
            }

            try
            {

                business.IsDeleted = true;

                business.Status = "Inactive";

               _context.Businesses.Update(business);
               await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Business deactivated successfully.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting this business.";
            }

            return RedirectToAction(nameof(Index));
        }
        //[HttpGet]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var business = await _context.Businesses
        //        .Include(b => b.Branches)
        //        .Include(b => b.Items)
        //        .FirstOrDefaultAsync(m => m.BusinessId == id);

        //    if (business == null)
        //        return NotFound();

        //    return View(business);
        //}
        
        #endregion
    }
}
