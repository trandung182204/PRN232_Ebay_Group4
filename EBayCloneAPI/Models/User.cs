using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Models;

[Table("User")]
[Index("Email", Name = "UQ__User__AB6E61646DF20411", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("username")]
    [StringLength(100)]
    public string? Username { get; set; }

    [Column("email")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Column("password")]
    [StringLength(255)]
    public string? Password { get; set; }

    [Column("role")]
    [StringLength(20)]
    public string? Role { get; set; }

    [Column("avatarURL")]
    public string? AvatarUrl { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    [InverseProperty("Bidder")]
    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    [InverseProperty("RaisedByNavigation")]
    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    [InverseProperty("Seller")]
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    [InverseProperty("Receiver")]
    public virtual ICollection<Message> MessageReceivers { get; set; } = new List<Message>();

    [InverseProperty("Sender")]
    public virtual ICollection<Message> MessageSenders { get; set; } = new List<Message>();

    [InverseProperty("Buyer")]
    public virtual ICollection<OrderTable> OrderTables { get; set; } = new List<OrderTable>();

    [InverseProperty("User")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("Seller")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [InverseProperty("User")]
    public virtual ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();

    [InverseProperty("Reviewer")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    [InverseProperty("Seller")]
    public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
}
