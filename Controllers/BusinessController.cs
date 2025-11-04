using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PRISM.Controllers
{
    public class BusinessController : Controller
    {
        private readonly AppDbContext _context;
        public BusinessController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var businesses = _context.Businesses.OrderBy(b => b.Name);
            return View(businesses.ToList());
        }
        [HttpGet]
        public IActionResult Details(int id)
        {
            var business = _context.Businesses.FirstOrDefault(b => b.BusinessId == id);
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
            if (!ModelState.IsValid)
            {
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
        public IActionResult Delete(int id)
        {
            var business = _context.Businesses.FirstOrDefault(m => m.BusinessId == id);
            if (business is null)
            {
                return NotFound();
            }
            _context.Businesses.Remove(business);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
