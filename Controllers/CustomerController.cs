using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PRISM.Models;

namespace PRISM.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Customer
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers
                .Include(c => c.Branch)
                    .ThenInclude(b => b.Business)
                .OrderBy(c => c.FullName)
                .ToListAsync();

            return View(customers);
        }

        // GET: Customer/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Branch)
                    .ThenInclude(b => b.Business)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

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
            var branches = await _context.Branches
                .Include(b => b.Business)
                .Where(b => !b.IsDeleted)
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
            // Remove navigation property from validation
            ModelState.Remove("Branch");

            if (!ModelState.IsValid)
            {
                // Reload branches if validation fails
                ViewBag.Branches = new SelectList(
                    await _context.Branches
                        .Include(b => b.Business)
                        .Where(b => !b.IsDeleted)
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
                // Verify the branch exists
                var branchExists = await _context.Branches
                    .AnyAsync(b => b.BranchId == customer.BranchId && !b.IsDeleted);

                if (!branchExists)
                {
                    ModelState.AddModelError("BranchId", "Selected branch does not exist.");
                    ViewBag.Branches = new SelectList(
                        await _context.Branches
                            .Include(b => b.Business)
                            .Where(b => !b.IsDeleted)
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
                            .Where(b => !b.IsDeleted)
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
                // Log the error
                Console.WriteLine($"Error creating customer: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                ModelState.AddModelError("", $"Error creating customer: {ex.Message}");
                ViewBag.Branches = new SelectList(
                    await _context.Branches
                        .Include(b => b.Business)
                        .Where(b => !b.IsDeleted)
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
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var branches = await _context.Branches
                .Include(b => b.Business)
                .Where(b => !b.IsDeleted)
                .Select(b => new
                {
                    b.BranchId,
                    DisplayName = b.Name + " - " + b.Business.Name
                })
                .ToListAsync();

            ViewBag.BranchId = new SelectList(branches, "BranchId", "DisplayName");

            return View(customer);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            // Remove navigation property from validation
            ModelState.Remove("Branch");

            if (!ModelState.IsValid)
            {
                ViewBag.Branches = new SelectList(
                    await _context.Branches
                        .Include(b => b.Business)
                        .Where(b => !b.IsDeleted)
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
                // Check for duplicate email (excluding current customer)
                var emailExists = await _context.Customers
                    .AnyAsync(c => c.Email.ToLower() == customer.Email.ToLower()
                              && c.CustomerId != customer.CustomerId);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "A customer with this email already exists.");
                    ViewBag.Branches = new SelectList(
                        await _context.Branches
                            .Include(b => b.Business)
                            .Where(b => !b.IsDeleted)
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

        // GET: Customer/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Branch)
                    .ThenInclude(b => b.Business)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
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

        // GET: Customer/Search
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction(nameof(Index));
            }

            var customers = await _context.Customers
                .Include(c => c.Branch)
                    .ThenInclude(b => b.Business)
                .Where(c => c.FullName.Contains(query)
                         || c.Email.Contains(query)
                         || c.Phone.Contains(query))
                .OrderBy(c => c.FullName)
                .ToListAsync();

            ViewBag.SearchQuery = query;
            return View("Index", customers);
        }

        private async Task<bool> CustomerExists(int id)
        {
            return await _context.Customers.AnyAsync(e => e.CustomerId == id);
        }
    }
}