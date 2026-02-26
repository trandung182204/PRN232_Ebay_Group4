using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Models;

[Table("Review")]
public partial class Review
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("productId")]
    public int? ProductId { get; set; }

    [Column("reviewerId")]
    public int? ReviewerId { get; set; }

    [Column("rating")]
    public int? Rating { get; set; }

    [Column("comment")]
    public string? Comment { get; set; }

    [Column("createdAt", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("Reviews")]
    public virtual Product? Product { get; set; }

    [ForeignKey("ReviewerId")]
    [InverseProperty("Reviews")]
    public virtual User? Reviewer { get; set; }
}
