using PRISM.Repositories.IRepositories;
namespace PRISM.Services.IServices
{
    public interface IUnitOfWork
    {

        IRepository<Order> Orders { get; }
        IRepository<Business> Businesses { get; }
        IRepository<Branch> Branches { get; }
        IRepository<Customer> Customers { get; }
        IRepository<Items> Items { get; }
        IRepository<OrderItem> OrderItems { get; }  


        Task<int> SaveAsync();
        int Save();       

    }
}
