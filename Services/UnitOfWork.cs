using Microsoft.EntityFrameworkCore.Storage;
using PRISM.DataAccess;
using PRISM.Repositories;
using PRISM.Repositories.IRepositories;
using PRISM.Services.IServices;
namespace PRISM.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }
        private IRepository<Order>? _orders;
        public IRepository<Order> Orders => _orders ??= new Repository<Order>(_context);

        public IRepository<Business>? _businesses;
        public IRepository<Business> Businesses => _businesses ??= new Repository<Business>(_context);

        public IRepository<Branch>? _branches;
        public IRepository<Branch> Branches => _branches ??= new Repository<Branch>(_context);

        public IRepository<Customer>? _customers;
        public IRepository<Customer> Customers => _customers ??= new Repository<Customer>(_context);

        public IRepository<Items>? _items;
        public IRepository<Items> Items => _items ??= new Repository<Items>(_context);

        public IRepository<OrderItem>? _ordersItems;
        public IRepository<OrderItem> OrderItems => _ordersItems ??= new Repository<OrderItem>(_context);
         


        public async Task<int> SaveAsync()=> await _context.SaveChangesAsync();
        
        public int Save()
        {
            return _context.SaveChanges();
        }
        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }

    }
}
