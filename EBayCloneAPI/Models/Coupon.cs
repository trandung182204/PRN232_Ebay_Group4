using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Models;

[Table("Coupon")]
public partial class Coupon
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string? Code { get; set; }

    [Column("discountPercent", TypeName = "decimal(5, 2)")]
    public decimal? DiscountPercent { get; set; }

    [Column("startDate", TypeName = "datetime")]
    public DateTime? StartDate { get; set; }

    [Column("endDate", TypeName = "datetime")]
    public DateTime? EndDate { get; set; }

    [Column("maxUsage")]
    public int? MaxUsage { get; set; }

    [Column("productId")]
    public int? ProductId { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("Coupons")]
    public virtual Product? Product { get; set; }
}
