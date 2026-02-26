using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Models;

[Table("Feedback")]
public partial class Feedback
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("sellerId")]
    public int? SellerId { get; set; }

    [Column("averageRating", TypeName = "decimal(3, 2)")]
    public decimal? AverageRating { get; set; }

    [Column("totalReviews")]
    public int? TotalReviews { get; set; }

    [Column("positiveRate", TypeName = "decimal(5, 2)")]
    public decimal? PositiveRate { get; set; }

    [ForeignKey("SellerId")]
    [InverseProperty("Feedbacks")]
    public virtual User? Seller { get; set; }
}
