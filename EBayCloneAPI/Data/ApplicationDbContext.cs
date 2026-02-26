using System;
using System.Collections.Generic;
using EBayCloneAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Bid> Bids { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<Dispute> Disputes { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderTable> OrderTables { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ReturnRequest> ReturnRequests { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<ShippingInfo> ShippingInfos { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // If options not already configured via DI, keep previous behavior (scaffolded).
        if (!optionsBuilder.IsConfigured)
        {
            // Leave blank so the DbContext can be configured from DI in Program.cs
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Address__3213E83FF44E653E");

            entity.HasOne(d => d.User).WithMany(p => p.Addresses).HasConstraintName("FK__Address__userId__3A81B327");
        });

        modelBuilder.Entity<Bid>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bid__3213E83F60965830");

            entity.HasOne(d => d.Bidder).WithMany(p => p.Bids).HasConstraintName("FK__Bid__bidderId__5629CD9C");

            entity.HasOne(d => d.Product).WithMany(p => p.Bids).HasConstraintName("FK__Bid__productId__5535A963");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3213E83F4BBBE4E2");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Coupon__3213E83F1B370BB5");

            entity.HasOne(d => d.Product).WithMany(p => p.Coupons).HasConstraintName("FK__Coupon__productI__60A75C0F");
        });

        modelBuilder.Entity<Dispute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Dispute__3213E83FDA898305");

            entity.HasOne(d => d.Order).WithMany(p => p.Disputes).HasConstraintName("FK__Dispute__orderId__693CA210");

            entity.HasOne(d => d.RaisedByNavigation).WithMany(p => p.Disputes).HasConstraintName("FK__Dispute__raisedB__6A30C649");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Feedback__3213E83F056BC7D6");

            entity.HasOne(d => d.Seller).WithMany(p => p.Feedbacks).HasConstraintName("FK__Feedback__seller__66603565");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventor__3213E83F41851574");

            entity.HasOne(d => d.Product).WithMany(p => p.Inventories).HasConstraintName("FK__Inventory__produ__6383C8BA");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Message__3213E83FEE357793");

            entity.HasOne(d => d.Receiver).WithMany(p => p.MessageReceivers).HasConstraintName("FK__Message__receive__5DCAEF64");

            entity.HasOne(d => d.Sender).WithMany(p => p.MessageSenders).HasConstraintName("FK__Message__senderI__5CD6CB2B");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderIte__3213E83FAB46694C");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems).HasConstraintName("FK__OrderItem__order__46E78A0C");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems).HasConstraintName("FK__OrderItem__produ__47DBAE45");
        });

        modelBuilder.Entity<OrderTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderTab__3213E83FC48BA5FB");

            entity.HasOne(d => d.Address).WithMany(p => p.OrderTables).HasConstraintName("FK__OrderTabl__addre__440B1D61");

            entity.HasOne(d => d.Buyer).WithMany(p => p.OrderTables).HasConstraintName("FK__OrderTabl__buyer__4316F928");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payment__3213E83FE9520EF3");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments).HasConstraintName("FK__Payment__orderId__4AB81AF0");

            entity.HasOne(d => d.User).WithMany(p => p.Payments).HasConstraintName("FK__Payment__userId__4BAC3F29");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3213E83F84775789");

            entity.HasOne(d => d.Category).WithMany(p => p.Products).HasConstraintName("FK__Product__categor__3F466844");

            entity.HasOne(d => d.Seller).WithMany(p => p.Products).HasConstraintName("FK__Product__sellerI__403A8C7D");
        });

        modelBuilder.Entity<ReturnRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReturnRe__3213E83F618A3145");

            entity.HasOne(d => d.Order).WithMany(p => p.ReturnRequests).HasConstraintName("FK__ReturnReq__order__5165187F");

            entity.HasOne(d => d.User).WithMany(p => p.ReturnRequests).HasConstraintName("FK__ReturnReq__userI__52593CB8");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Review__3213E83F36C26AB9");

            entity.HasOne(d => d.Product).WithMany(p => p.Reviews).HasConstraintName("FK__Review__productI__59063A47");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.Reviews).HasConstraintName("FK__Review__reviewer__59FA5E80");
        });

        modelBuilder.Entity<ShippingInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Shipping__3213E83FE71CFCE4");

            entity.HasOne(d => d.Order).WithMany(p => p.ShippingInfos).HasConstraintName("FK__ShippingI__order__4E88ABD4");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Store__3213E83FCD584930");

            entity.HasOne(d => d.Seller).WithMany(p => p.Stores).HasConstraintName("FK__Store__sellerId__6D0D32F4");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3213E83F073F4234");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
