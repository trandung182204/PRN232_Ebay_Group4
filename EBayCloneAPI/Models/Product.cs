using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Models;

[Table("Product")]
public partial class Product
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string? Title { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("price", TypeName = "decimal(10, 2)")]
    public decimal? Price { get; set; }

    [Column("images")]
    public string? Images { get; set; }

    [Column("categoryId")]
    public int? CategoryId { get; set; }

    [Column("sellerId")]
    public int? SellerId { get; set; }

    [Column("isAuction")]
    public bool? IsAuction { get; set; }

    [Column("auctionEndTime", TypeName = "datetime")]
    public DateTime? AuctionEndTime { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category? Category { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();

    [InverseProperty("Product")]
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    [InverseProperty("Product")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [InverseProperty("Product")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    [ForeignKey("SellerId")]
    [InverseProperty("Products")]
    public virtual User? Seller { get; set; }
}
