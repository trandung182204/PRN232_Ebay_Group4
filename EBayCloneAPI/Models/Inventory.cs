using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Models;

[Table("Inventory")]
public partial class Inventory
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("productId")]
    public int? ProductId { get; set; }

    [Column("quantity")]
    public int? Quantity { get; set; }

    [Column("lastUpdated", TypeName = "datetime")]
    public DateTime? LastUpdated { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("Inventories")]
    public virtual Product? Product { get; set; }
}
