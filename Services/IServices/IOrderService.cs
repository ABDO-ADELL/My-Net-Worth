namespace PRISM.Services.IServices
{
    public interface IOrderService
    {
        
        Task<IEnumerable<Order>> GetAllOrdersAsync(string userId);
        Task<Order?> GetOrderByIdAsync(int id, string userId);
        Task<Order?> GetOrderDetailsAsync(int id, string userId);
        Task<(bool Success, string Message, Order? Order)> CreateOrderAsync(Order order, List<int> itemIds, List<int> quantities, string userId);
        Task<(bool Success, string Message)> UpdateOrderAsync(int id, Order order, string userId);
        Task<(bool Success, string Message)> DeleteOrderAsync(int id, string userId);
        Task<IEnumerable<Branch>> GetBranchesByBusinessAsync(int businessId, string userId);
        Task<IEnumerable<Items>> GetItemsByBranchAsync(int branchId, string userId);
        Task<bool> OrderExistsAsync(int id, string userId);
    }

}

