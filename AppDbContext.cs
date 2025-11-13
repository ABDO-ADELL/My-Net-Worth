using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PRISM.Models.Authmodels;
using System.Reflection.Emit;


namespace PRISM
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=MyNetWorthdemo;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;");
        //}
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        public DbSet<ItemCategory> ItemCategories { get; set; }
        public DbSet<Items> Items { get; set; }
        public DbSet<Inventory> Inventories { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Payment> Payments { get; set; }

        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Target> Targets { get; set; }


        public DbSet<Business> Businesses { get; set; }
        public DbSet<Branch> Branches { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure decimal precision
            modelBuilder.Entity<Items>()
                .Property(i => i.CostPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Items>()
                .Property(i => i.SellPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.total_amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Target>()
                .Property(t => t.target_profit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Target>()
                .Property(t => t.target_revenue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            // Fix Branch relationship - IMPORTANT: Remove my previous Branch config
            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Business)
                .WithMany(bus => bus.Branches)
                .HasForeignKey(b => b.BusinessId)
                .OnDelete(DeleteBehavior.Cascade); // Branch should cascade with Business


            modelBuilder.Entity<AppUser>()
                .HasMany(b => b.Business)
                .WithOne(u => u.Users)
                .HasForeignKey(b => b.UserId)
                .HasPrincipalKey(b => b.Id)
                .OnDelete(DeleteBehavior.NoAction); 
            // Branch should cascade with Business

            // Fix Item relationships
            modelBuilder.Entity<Items>()
                .HasOne(i => i.ItemCategory)
                .WithMany()
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Items>()
                .HasOne(i => i.Branch)
                .WithMany(b => b.Items)
                .HasForeignKey(i => i.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fix Inventory relationship
            modelBuilder.Entity<Inventory>()
                .HasOne(inv => inv.Item)
                .WithMany()
                .HasForeignKey(inv => inv.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory>()
                .HasOne(inv => inv.Branch)
                .WithMany()
                .HasForeignKey(inv => inv.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.branch)
                .WithMany()
                .HasForeignKey(o => o.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItem relationship
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Item)
                .WithMany()
                .HasForeignKey(oi => oi.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment relationship
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }

    }
}
