using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PRISM.DataAccess;
using PRISM.Models;
using System.Security.Claims;

namespace PRISM.Controllers
{
    [Authorize]
    public class CustomerController : BaseController
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // GET: Customer
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            var customers = await _context.Customers
                .Include(c => c.Branch)
                    .ThenInclude(b => b.Business)
                .Where(c => c.Branch.Business.UserId == userId)
                .OrderBy(c => c.FullName)
                .ToListAsync();

            return View(customers);
        }

        // GET: Customer/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();

            var customer = await _context.Customers
                .Include(c => c.Branch)
                    .ThenInclude(b => b.Business)
                .FirstOrDefaultAsync(c => c.CustomerId == id
                    && c.Branch.Business.UserId == userId);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customer/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = GetCurrentUserId();

            var branches = await _context.Branches
                .Include(b => b.Business)
                .Where(b => !b.IsDeleted && b.Business.UserId == userId)
                .Select(b => new
                {
                    b.BranchId,
                    DisplayName = b.Name + " - " + b.Business.Name
                })
                .ToListAsync();

            ViewBag.BranchId = new SelectList(branches, "BranchId", "DisplayName");

            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            var userId = GetCurrentUserId();

            ModelState.Remove("Branch");

            if (!ModelState.IsValid)
            {
                ViewBag.Branches = new SelectList(
                    await _context.Branches
                        .Include(b => b.Business)
                        .Where(b => !b.IsDeleted && b.Business.UserId == userId)
                        .Select(b => new
                        {
                            b.BranchId,
                            DisplayName = b.Name + " - " + b.Business.Name
                        })
                        .ToListAsync(),
                    "BranchId",
                    "DisplayName",
                    customer.BranchId
                );

                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return View(customer);
            }

            try
            {
                // Verify the branch exists and belongs to user
                var branchExists = await _context.Branches
                    .Include(b => b.Business)
                    .AnyAsync(b => b.BranchId == customer.BranchId
                        && !b.IsDeleted
                        && b.Business.UserId == userId);

                if (!branchExists)
                {
                    ModelState.AddModelError("BranchId", "Selected branch does not exist.");
                    ViewBag.Branches = new SelectList(
                        await _context.Branches
                            .Include(b => b.Business)
                            .Where(b => !b.IsDeleted && b.Business.UserId == userId)
                            .Select(b => new
                            {
                                b.BranchId,
                                DisplayName = b.Name + " - " + b.Business.Name
                            })
                            .ToListAsync(),
                        "BranchId",
                        "DisplayName"
                    );
                    TempData["ErrorMessage"] = "Selected branch is invalid.";
                    return View(customer);
                }

                // Check for duplicate email
                var emailExists = await _context.Customers
                    .AnyAsync(c => c.Email.ToLower() == customer.Email.ToLower());

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "A customer with this email already exists.");
                    ViewBag.Branches = new SelectList(
                        await _context.Branches
                            .Include(b => b.Business)
                            .Where(b => !b.IsDeleted && b.Business.UserId == userId)
                            .Select(b => new
                            {
                                b.BranchId,
                                DisplayName = b.Name + " - " + b.Business.Name
                            })
                            .ToListAsync(),
                        "BranchId",
                        "DisplayName",
                        customer.BranchId
                    );
                    TempData["ErrorMessage"] = "Email already exists.";
                    return View(customer);
                }

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Customer '{customer.FullName}' added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating customer: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                ModelState.AddModelError("", $"Error creating customer: {ex.Message}");
                ViewBag.Branches = new SelectList(
                    await _context.Branches
                        .Include(b => b.Business)
                        .Where(b => !b.IsDeleted && b.Business.UserId == userId)
                        .Select(b => new
                        {
                            b.BranchId,
                            DisplayName = b.Name + " - " + b.Business.Name
                        })
                        .ToListAsync(),
                    "BranchId",
                    "DisplayName",
                    customer.BranchId
                );
                TempData["ErrorMessage"] = "An error occurred while creating the customer.";
                return View(customer);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();

            var customer = await _context.Customers
                .Include(c => c.Branch)
                    .ThenInclude(b => b.Business)
                .FirstOrDefaultAsync(c => c.CustomerId == id
                    && c.Branch.Business.UserId == userId);

            if (customer == null)
            {
                return NotFound();
            }

            var branches = await _context.Branches
                .Include(b => b.Business)
                .Where(b => !b.IsDeleted && b.Business.UserId == userId)
                .Select(b => new
                {
                    b.BranchId,
                    DisplayName = b.Name + " - " + b.Business.Name
                })
                .ToListAsync();

            ViewBag.BranchId = new SelectList(branches, "BranchId", "DisplayName");

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            var userId = GetCurrentUserId();

            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            ModelState.Remove("Branch");

            if (!ModelState.IsValid)
            {
                ViewBag.Branches = new SelectList(
                    await _context.Branches
                        .Include(b => b.Business)
                        .Where(b => !b.IsDeleted && b.Business.UserId == userId)
                        .Select(b => new
                        {
                            b.BranchId,
                            DisplayName = b.Name + " - " + b.Business.Name
                        })
                        .ToListAsync(),
                    "BranchId",
                    "DisplayName",
                    customer.BranchId
                );
                return View(customer);
            }

            try
            {
                // Verify customer belongs to user's business
                var existingCustomer = await _context.Customers
                    .Include(c => c.Branch)
                        .ThenInclude(b => b.Business)
                    .FirstOrDefaultAsync(c => c.CustomerId == id
                        && c.Branch.Business.UserId == userId);

                if (existingCustomer == null)
                {
                    return NotFound();
                }

                // Check for duplicate email
                var emailExists = await _context.Customers
                    .AnyAsync(c => c.Email.ToLower() == customer.Email.ToLower()
                              && c.CustomerId != customer.CustomerId);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "A customer with this email already exists.");
                    ViewBag.Branches = new SelectList(
                        await _context.Branches
                            .Include(b => b.Business)
                            .Where(b => !b.IsDeleted && b.Business.UserId == userId)
                            .Select(b => new
                            {
                                b.BranchId,
                                DisplayName = b.Name + " - " + b.Business.Name
                            })
                            .ToListAsync(),
                        "BranchId",
                        "DisplayName",
                        customer.BranchId
                    );
                    TempData["ErrorMessage"] = "Email already exists.";
                    return View(customer);
                }

                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Customer '{customer.FullName}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CustomerExists(customer.CustomerId))
                {
                    return NotFound();
                }
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();

            var customer = await _context.Customers
                .Include(c => c.Branch)
                    .ThenInclude(b => b.Business)
                .FirstOrDefaultAsync(c => c.CustomerId == id
                    && c.Branch.Business.UserId == userId);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();

            var customer = await _context.Customers
                .Include(c => c.Branch)
                    .ThenInclude(b => b.Business)
                .FirstOrDefaultAsync(c => c.CustomerId == id
                    && c.Branch.Business.UserId == userId);

            if (customer == null)
            {
                return NotFound();
            }

            // Check if customer has any orders
            var hasOrders = await _context.Orders
                .AnyAsync(o => o.CustomerId == id && !o.IsDeleted);

            if (hasOrders)
            {
                TempData["ErrorMessage"] = $"Cannot delete customer '{customer.FullName}' because they have existing orders.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Customer '{customer.FullName}' deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting customer: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while deleting the customer.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction(nameof(Index));
            }

            var customers = await _context.Customers
                .Include(c => c.Branch)
                    .ThenInclude(b => b.Business)
                .Where(c => c.Branch.Business.UserId == userId
                         && (c.FullName.Contains(query)
                         || c.Email.Contains(query)
                         || c.Phone.Contains(query)))
                .OrderBy(c => c.FullName)
                .ToListAsync();

            ViewBag.SearchQuery = query;
            return View("Index", customers);
        }

        private async Task<bool> CustomerExists(int id)
        {
            var userId = GetCurrentUserId();
            return await _context.Customers
                .Include(c => c.Branch)
                    .ThenInclude(b => b.Business)
                .AnyAsync(e => e.CustomerId == id
                    && e.Branch.Business.UserId == userId);
        }
    }
}