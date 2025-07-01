using AutoOtpad.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<TestResult> TestResults { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<ReturnRequest> ReturnRequests { get; set; }
    public DbSet<QualityLog> QualityLogs { get; set; }
    public DbSet<Promotion> Promotions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User - Order (1:N)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId);

        // Order - OrderItem (1:N)
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId);

        // OrderItem - Part (N:1)
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Part)
            .WithMany()
            .HasForeignKey(oi => oi.PartId);

        // Part - TestResult (1:N)
        modelBuilder.Entity<TestResult>()
            .HasOne(tr => tr.Part)
            .WithMany()
            .HasForeignKey(tr => tr.PartId);

        // Message - Sender/Receiver (self-referencing)
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        // Invoice - Order (1:1)
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Order)
            .WithOne()
            .HasForeignKey<Invoice>(i => i.OrderId);

        // ReturnRequest - OrderItem (1:1)
        modelBuilder.Entity<ReturnRequest>()
            .HasOne(rr => rr.OrderItem)
            .WithMany()
            .HasForeignKey(rr => rr.OrderItemId);

        // QualityLog - Part (1:N)
        modelBuilder.Entity<QualityLog>()
            .HasOne(ql => ql.Part)
            .WithMany()
            .HasForeignKey(ql => ql.PartId);

        // Promotion - Part (optional 1:N)
        modelBuilder.Entity<Promotion>()
            .HasOne(p => p.Part)
            .WithMany()
            .HasForeignKey(p => p.PartId)
            .IsRequired(false);
    }
}
