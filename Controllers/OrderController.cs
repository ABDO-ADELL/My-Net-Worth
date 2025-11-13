using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PRISM.Models;
using PRISM.Models.Authmodels;
using System.Security.Claims;

namespace PRISM.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public OrderController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // GET: Order
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _context.Orders
                .Include(o => o.business)
                .Include(o => o.branch)
                .Include(o => o.Customer)
                .Include(o => o.user)
                .Where(o => !o.IsDeleted && o.UserId == userId)
                .OrderByDescending(o => o.datetime)
                .ToListAsync();

            return View(orders);
        }
        // GET: Order/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.business)
                .Include(o => o.branch)
                .Include(o => o.Customer)
                .Include(o => o.user)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Order/Create
        public IActionResult Create()
        {
            ViewData["business_id"] = new SelectList(_context.Businesses, "BusinessId", "Name");
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Name");
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "FullName");
            ViewData["Items"] = _context.Items
                .Where(i => !i.IsDeleted)
                .Select(i => new { i.ItemId, i.Name, i.SellPrice })
                .ToList();

            return View();
        }

        // POST: Order/Create
        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order, List<int> itemIds, List<int> quantities)
        {
            try
            {
                // Get current user and set the UserId (FK)
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    order.UserId = user.Id;  // Set FK, not navigation property
                }

                order.datetime = DateTime.Now;
                order.IsDeleted = false;
                order.status = true;

                // Check if items were added
                if (itemIds == null || itemIds.Count == 0 || quantities == null || quantities.Count == 0)
                {
                    ModelState.AddModelError("", "Please add at least one item to the order.");
                }

                if (ModelState.IsValid)
                {
                    // Calculate total amount and create order items

                    decimal totalAmount = 0;
                    var orderItems = new List<OrderItem>();

                    for (int i = 0; i < itemIds.Count; i++)
                    {
                        if (quantities[i] > 0)
                        {
                            var item = await _context.Items.FindAsync(itemIds[i]);
                            if (item != null)
                            {
                                var orderItem = new OrderItem
                                {
                                    ItemId = itemIds[i],
                                    Quantity = quantities[i],
                                    Price = item.SellPrice,
                                    TotalPrice = item.SellPrice * quantities[i]
                                };
                                orderItems.Add(orderItem);
                                totalAmount += orderItem.TotalPrice;
                            }
                        }
                    }

                    order.total_amount = totalAmount;
                    order.OrderItems = orderItems;

                    // Add order to context
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Order created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Log validation errors for debugging
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    TempData["Error"] = "Validation failed: " + string.Join(", ", errors);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating order: " + ex.Message);
                TempData["Error"] = "Error: " + ex.Message + " - " + ex.InnerException?.Message;
            }

            // Reload dropdown data
            ViewData["business_id"] = new SelectList(_context.Businesses, "BusinessId", "Name", order.BusinessId);
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Name", order.BranchId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "FullName", order.CustomerId);
            ViewData["Items"] = _context.Items
                .Where(i => !i.IsDeleted)
                .Select(i => new { i.ItemId, i.Name, i.SellPrice })
                .ToList();

            return View(order);
        }        // GET: Order/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

            if (order == null)
            {
                return NotFound();
            }

            ViewData["business_id"] = new SelectList(_context.Businesses, "BusinessId", "Name", order.BusinessId);
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Name", order.BranchId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "FullName", order.CustomerId);

            return View(order);
        }

        // POST: Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Order updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["business_id"] = new SelectList(_context.Businesses, "BusinessId", "Name", order.business);
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Name", order.BranchId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "FullName", order.CustomerId);

            return View(order);
        }

        // GET: Order/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.business)
                .Include(o => o.branch)
                .Include(o => o.Customer)
                .Include(o => o.user)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                // Soft delete
                order.IsDeleted = true;
                _context.Update(order);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Order deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // API endpoint to get branches by business
        [HttpGet]
        public JsonResult GetBranchesByBusiness(int businessId)
        {
            var branches = _context.Branches
                .Where(b => b.BusinessId == businessId)
                .Select(b => new { b.BranchId, b.Name })
                .ToList();

            return Json(branches);
        }

        // API endpoint to get items by branch
        [HttpGet]
        public JsonResult GetItemsByBranch(int branchId)
        {
            var items = _context.Items
                .Where(i => i.BranchId == branchId && !i.IsDeleted)
                .Select(i => new { i.ItemId, i.Name, i.SellPrice })
                .ToList();

            return Json(items);
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}