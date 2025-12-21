using Microsoft.EntityFrameworkCore;
using PRISM.Repositories.IRepositories;
using PRISM.Models;
using System.Linq.Expressions;
using PRISM.Services.IServices;

namespace PRISM.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync(string userId)
        {
            var orders = await _unitOfWork.Orders.GetAsync(
                expression: o => !o.IsDeleted && o.business.UserId == userId,
                includes: new Expression<Func<Order, object>>[]
                {
                    o => o.business,
                    o => o.branch,
                    o => o.Customer,
                    o => o.user
                }
            );

            return orders.OrderByDescending(o => o.datetime);
        }

        public async Task<Order?> GetOrderByIdAsync(int id, string userId)
        {
            return await _unitOfWork.Orders.GetOneAsync(
                expression: o => o.Id == id && !o.IsDeleted && o.business.UserId == userId,
                includes: new Expression<Func<Order, object>>[]
                {
                    o => o.business,
                    o => o.branch,
                    o => o.Customer,
                    o => o.user
                }
            );
        }

        public async Task<Order?> GetOrderDetailsAsync(int id, string userId)
        {
            return await _unitOfWork.Orders.GetOneAsync(
                expression: o => o.Id == id && !o.IsDeleted && o.business.UserId == userId,
                includes: new Expression<Func<Order, object>>[]
                {
                    o => o.business,
                    o => o.branch,
                    o => o.Customer,
                    o => o.user,
                    o => o.OrderItems
                }
            );
        }

        public async Task<(bool Success, string Message, Order? Order)> CreateOrderAsync(
            Order order,
            List<int> itemIds,
            List<int> quantities,
            string userId)
        {
            // Validate items
            if (itemIds == null || itemIds.Count == 0 || quantities == null || quantities.Count == 0)
            {
                return (false, "Please add at least one item to the order.", null);
            }

            // Validate item counts match
            if (itemIds.Count != quantities.Count)
            {
                return (false, "Item IDs and quantities count mismatch.", null);
            }

            try
            {
                //await _unitOfWork.BeginTransactionAsync();

                // Set order properties
                order.UserId = userId;
                order.IsDeleted = false;
                order.status = true;
                if (order.datetime == DateTime.MinValue || order.datetime == default)
                {
                    order.datetime = DateTime.UtcNow;
                }

                // Verify business belongs to user
                var business = await _unitOfWork.Businesses.GetOneAsync(
                    b => b.BusinessId == order.BusinessId && b.UserId == userId && !b.IsDeleted
                );

                if (business == null)
                {
                    return (false, "Invalid business selected.", null);
                }

                // Calculate total amount and create order items
                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                for (int i = 0; i < itemIds.Count; i++)
                {
                    if (quantities[i] > 0)
                    {
                        var item = await _unitOfWork.Items.GetOneAsync(
                            it => it.ItemId == itemIds[i] && !it.IsDeleted
                        );

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

                if (orderItems.Count == 0)
                {
                    return (false, "No valid items selected.", null);
                }

                order.total_amount = totalAmount;
                order.OrderItems = orderItems;

                // Add order
                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveAsync();
                //await _unitOfWork.CommitTransactionAsync();

                return (true, "Order created successfully!", order);
            }
            catch (Exception ex)
            {
                //await _unitOfWork.RollbackTransactionAsync();
                return (false, $"Error creating order: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateOrderAsync(int id, Order order, string userId)
        {
            try
            {
                // Verify order exists and belongs to user
                var existingOrder = await _unitOfWork.Orders.GetOneAsync(
                    o => o.Id == id && !o.IsDeleted && o.business.UserId == userId,
                    includes: new Expression<Func<Order, object>>[]
                    {
                        o => o.business
                    }
                );

                if (existingOrder == null)
                {
                    return (false, "Order not found or access denied.");
                }

                // Update properties
                existingOrder.OrderName = order.OrderName;
                existingOrder.BusinessId = order.BusinessId;
                existingOrder.BranchId = order.BranchId;
                existingOrder.CustomerId = order.CustomerId;
                existingOrder.status = order.status;
                existingOrder.total_amount = order.total_amount;

                _unitOfWork.Orders.Update(existingOrder);
                await _unitOfWork.SaveAsync();

                return (true, "Order updated successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating order: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteOrderAsync(int id, string userId)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetOneAsync(
                    o => o.Id == id && !o.IsDeleted && o.business.UserId == userId,
                    includes: new Expression<Func<Order, object>>[]
                    {
                        o => o.business
                    }
                );

                if (order == null)
                {
                    return (false, "Order not found or access denied.");
                }

                // Soft delete
                order.IsDeleted = true;
                _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveAsync();

                return (true, "Order deleted successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting order: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Branch>> GetBranchesByBusinessAsync(int businessId, string userId)
        {
            return await _unitOfWork.Branches.GetAsync(
                expression: b => b.BusinessId == businessId && !b.IsDeleted && b.Business.UserId == userId
            );
        }

        public async Task<IEnumerable<Items>> GetItemsByBranchAsync(int branchId, string userId)
        {
            return await _unitOfWork.Items.GetAsync(
                expression: i => i.BranchId == branchId && !i.IsDeleted && i.Business.UserId == userId
            );
        }

        public async Task<bool> OrderExistsAsync(int id, string userId)
        {
            var order = await _unitOfWork.Orders.GetOneAsync(
                o => o.Id == id && !o.IsDeleted && o.business.UserId == userId,
                includes: new Expression<Func<Order, object>>[]
                {
                    o => o.business
                }
            );

            return order != null;
        }


    }
}