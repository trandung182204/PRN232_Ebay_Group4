using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Models;

[Table("ShippingInfo")]
public partial class ShippingInfo
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("orderId")]
    public int? OrderId { get; set; }

    [Column("carrier")]
    [StringLength(100)]
    public string? Carrier { get; set; }

    [Column("trackingNumber")]
    [StringLength(100)]
    public string? TrackingNumber { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string? Status { get; set; }

    [Column("estimatedArrival", TypeName = "datetime")]
    public DateTime? EstimatedArrival { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("ShippingInfos")]
    public virtual OrderTable? Order { get; set; }
}
