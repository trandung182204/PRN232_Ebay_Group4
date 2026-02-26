using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Models;

[Table("Bid")]
public partial class Bid
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("productId")]
    public int? ProductId { get; set; }

    [Column("bidderId")]
    public int? BidderId { get; set; }

    [Column("amount", TypeName = "decimal(10, 2)")]
    public decimal? Amount { get; set; }

    [Column("bidTime", TypeName = "datetime")]
    public DateTime? BidTime { get; set; }

    [ForeignKey("BidderId")]
    [InverseProperty("Bids")]
    public virtual User? Bidder { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("Bids")]
    public virtual Product? Product { get; set; }
}
