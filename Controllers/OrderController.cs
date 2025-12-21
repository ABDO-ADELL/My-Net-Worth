using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PRISM.DataAccess;
using PRISM.Repositories.IRepositories;
using PRISM.Models;
using PRISM.Services.IServices;
using System.Security.Claims;

namespace PRISM.Controllers
{
    [Authorize]
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IUnitOfWork _unitOfWork;
        private AppDbContext _context;

        public OrderController(AppDbContext context, IOrderService orderService, IUnitOfWork unitOfWork )
            : base(context)
        {
            _context = context;
            _orderService = orderService;
            _unitOfWork = unitOfWork;
        }

        // GET: Order
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var orders = await _orderService.GetAllOrdersAsync(userId);
            return View(orders);
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            var order = await _orderService.GetOrderDetailsAsync(id.Value, userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Order/Create
        public async Task<IActionResult> Create()
        {
            var model = new Order
            {
                datetime = DateTime.UtcNow
            };
            await PopulateDropdowns();
            return View(model);
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order, List<int> itemIds, List<int> quantities)
        {
            var userId = GetCurrentUserId();

            // Remove navigation properties from model validation
            ModelState.Remove("user");
            ModelState.Remove("business");
            ModelState.Remove("branch");
            ModelState.Remove("Customer");
            ModelState.Remove("OrderItems");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(order);
            }

            var result = await _orderService.CreateOrderAsync(order, itemIds, quantities, userId);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = result.Message;
            await PopulateDropdowns();
            return View(order);
        }

        // GET: Order/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            var order = await _orderService.GetOrderByIdAsync(id.Value, userId);

            if (order == null)
            {
                return NotFound();
            }

            await PopulateDropdowns();
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

            var userId = GetCurrentUserId();

            // Remove navigation properties from model validation
            ModelState.Remove("user");
            ModelState.Remove("business");
            ModelState.Remove("branch");
            ModelState.Remove("Customer");
            ModelState.Remove("OrderItems");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(order);
            }

            var result = await _orderService.UpdateOrderAsync(id, order, userId);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = result.Message;
            await PopulateDropdowns();
            return View(order);
        }

        // GET: Order/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            var order = await _orderService.GetOrderByIdAsync(id.Value, userId);

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
            var userId = GetCurrentUserId();
            var result = await _orderService.DeleteOrderAsync(id, userId);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // API endpoint to get branches by business
        [HttpGet]
        public async Task<JsonResult> GetBranchesByBusiness(int businessId)
        {
            var userId = GetCurrentUserId();
            var branches = await _orderService.GetBranchesByBusinessAsync(businessId, userId);

            var result = branches.Select(b => new { b.BranchId, b.Name });
            return Json(result);
        }

        // API endpoint to get items by branch
        [HttpGet]
        public async Task<JsonResult> GetItemsByBranch(int branchId)
        {
            var userId = GetCurrentUserId();
            var items = await _orderService.GetItemsByBranchAsync(branchId, userId);

            var result = items.Select(i => new { i.ItemId, i.Name, i.SellPrice });
            return Json(result);
        }

        private async Task PopulateDropdowns()
        {
            var userId = GetCurrentUserId();

            var businesses = await _unitOfWork.Businesses.GetAsync(
                expression: b => !b.IsDeleted && b.UserId == userId
            );

            ViewBag.BusinessId = new SelectList(businesses, "BusinessId", "Name");

            var branches = await _unitOfWork.Branches.GetAsync(
                expression: b => !b.IsDeleted && b.Business.UserId == userId,
                includes: new System.Linq.Expressions.Expression<Func<Branch, object>>[]
                {
                    b => b.Business
                }
            );

            ViewBag.BranchId = new SelectList(branches, "BranchId", "Name");

            var customers = await _unitOfWork.Customers.GetAsync(
                expression: c => c.Branch.Business.UserId == userId,
                includes: new System.Linq.Expressions.Expression<Func<Customer, object>>[]
                {
                    c => c.Branch,
                    c => c.Branch.Business
                }
            );

            ViewBag.CustomerId = new SelectList(customers, "CustomerId", "FullName");

            var items = await _unitOfWork.Items.GetAsync(
                expression: i => !i.IsDeleted && i.Business.UserId == userId,
                includes: new System.Linq.Expressions.Expression<Func<Items, object>>[]
                {
                    i => i.Business
                }
            );

            ViewData["Items"] = items.Select(i => new { i.ItemId, i.Name, i.SellPrice }).ToList();
        }
    }
}