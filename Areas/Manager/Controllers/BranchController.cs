using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRISM.Areas.Manager.Models;

namespace PRISM.Areas.Manager.Controllers
{
    public class BranchController : Controller
    {
        private readonly AppDbContext _context;
        public BranchController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {   
            var branch = _context.Branches.OrderBy(b=>b.Name);
            return View(branch.ToList());
        }
        #region Details
        [HttpGet]
        public IActionResult Details(int id)
        {
            var branch = _context.Branches.FirstOrDefault(b => b.BranchId == id);
            if (branch is null)
            {
                return NotFound();
                //RedirectToAction(SD.NotFoundPage,controllerName: SD.HomeController);
            }
            return View(branch);
        }
        #endregion

        #region Create  
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Branch branch)
        {
            if (!ModelState.IsValid)
            {
                return View(branch);
            }
            _context.Branches.Add(branch);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Edit    
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var branch = _context.Branches.FirstOrDefault(b => b.BranchId == id);
            if (branch is null)
            {
                return NotFound();
                //RedirectToAction(SD.NotFoundPage,controllerName: SD.HomeController);
            }
            return View(branch);
        }
        [HttpPost]
        public IActionResult Edit(Branch branch)
        {
            if (!ModelState.IsValid)
            {
                return View(branch);
            }
            _context.Branches.Update(branch);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Delete
        public IActionResult Delete(int id)
        {
            var branch = _context.Branches.FirstOrDefault(m => m.BranchId  == id);
            if (branch is null)
            {
                return NotFound();
            }
            _context.Branches.Remove(branch);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
