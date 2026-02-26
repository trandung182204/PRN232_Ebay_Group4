using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Models;

[Table("Store")]
public partial class Store
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("sellerId")]
    public int? SellerId { get; set; }

    [Column("storeName")]
    [StringLength(100)]
    public string? StoreName { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("bannerImageURL")]
    public string? BannerImageUrl { get; set; }

    [ForeignKey("SellerId")]
    [InverseProperty("Stores")]
    public virtual User? Seller { get; set; }
}
