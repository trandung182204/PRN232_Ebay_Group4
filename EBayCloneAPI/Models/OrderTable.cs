using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Models;

[Table("OrderTable")]
public partial class OrderTable
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("buyerId")]
    public int? BuyerId { get; set; }

    [Column("addressId")]
    public int? AddressId { get; set; }

    [Column("orderDate", TypeName = "datetime")]
    public DateTime? OrderDate { get; set; }

    [Column("totalPrice", TypeName = "decimal(10, 2)")]
    public decimal? TotalPrice { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string? Status { get; set; }

    [ForeignKey("AddressId")]
    [InverseProperty("OrderTables")]
    public virtual Address? Address { get; set; }

    [ForeignKey("BuyerId")]
    [InverseProperty("OrderTables")]
    public virtual User? Buyer { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [InverseProperty("Order")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("Order")]
    public virtual ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();

    [InverseProperty("Order")]
    public virtual ICollection<ShippingInfo> ShippingInfos { get; set; } = new List<ShippingInfo>();
}
