using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRISM.Models;

namespace PRISM.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        //[HttpPost("AddOrderItem")]
        public async Task<IActionResult> AddOrderItem(OrderItem model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid");
            }

            var order = await _context.Orders.FindAsync(model.OrderId);
            if (order == null)
                return NotFound("order not found");
            var item = await _context.Items.FindAsync(model.ItemId);
            if (item == null)
                return NotFound("Item not found");


            var NewItem = new OrderItem
            {
                Quantity = model.Quantity,
                ItemId = model.ItemId,
                Item = model.Item,
                Price = item.SellPrice, 
                TotalPrice = model.Quantity * item.SellPrice,
                OrderId = model.OrderId
            };
            _context.OrderItems.Add(NewItem);
            await _context.SaveChangesAsync();

            order.total_amount = await _context.OrderItems
              .Where(oi => oi.OrderId == model.OrderId)
              .SumAsync(oi => oi.TotalPrice);
        
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Orders", new { id = model.OrderId });

        }


        //[HttpDelete("DeleteOrderItem")]
        public async Task<IActionResult> DeleteOrderItem(int itemid)
        {
            var item = await _context.OrderItems.FindAsync(itemid);
            if (item == null)
                return NotFound();
            _context.OrderItems.Remove(item);
            await _context.SaveChangesAsync();

            var orderId = item.OrderId;
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.total_amount = await _context.OrderItems
                    .Where(oi => oi.OrderId == orderId)
                    .SumAsync(oi => oi.TotalPrice);
                await _context.SaveChangesAsync();
            }


            return RedirectToAction("Orders");
        }


        //[HttpGet("GetAllOrders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var Orders = await _context.Orders.ToListAsync();
            return View(Orders);
        }
        [HttpGet("OrderDetails")]
        public async Task<IActionResult> OrderDetails(int OrderId)
        {
            var Order = await _context.Orders.Include(i => i.branch).Include(i => i.business).Include(i => i.Customer).Include(i => i.OrderItems)
                            .ThenInclude(oi => oi.Item)   
                .FirstOrDefaultAsync(z => z.order_id == OrderId);

            if (Order == null)
                return NotFound();

            return View(Order);

        }
        //[HttpGet("SearchOrder")]
        public async Task<IActionResult> SearchOrder(string Name)
        {
            var order = await _context.Orders.Where(o => o.OrderName == Name).ToListAsync();
            if (!order.Any())
                return NotFound("No orders found.");

            return View(order);
        }
        //[HttpPost("EditOrder")]
        public async Task<IActionResult> EditOrder(Order model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Missing some Information");
            }
            var order = await _context.Orders.FirstOrDefaultAsync(i => i.order_id == model.order_id);
            if (order == null)
            {
                return NotFound("The Order dosen't exisit");
            }
            order.OrderName = model.OrderName;
            order.total_amount = model.total_amount;
            order.status = model.status;
            order.datetime = model.datetime;
            order.BranchId = model.BranchId;
            order.business_id = model.business_id;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Orders");
        }

        public IActionResult Index()
        {
            return View();
        }


    }
}
