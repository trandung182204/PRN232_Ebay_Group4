using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Models;

[Table("Payment")]
public partial class Payment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("orderId")]
    public int? OrderId { get; set; }

    [Column("userId")]
    public int? UserId { get; set; }

    [Column("amount", TypeName = "decimal(10, 2)")]
    public decimal? Amount { get; set; }

    [Column("method")]
    [StringLength(50)]
    public string? Method { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string? Status { get; set; }

    [Column("paidAt", TypeName = "datetime")]
    public DateTime? PaidAt { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("Payments")]
    public virtual OrderTable? Order { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Payments")]
    public virtual User? User { get; set; }
}
