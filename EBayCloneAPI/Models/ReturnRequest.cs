using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Models;

[Table("ReturnRequest")]
public partial class ReturnRequest
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("orderId")]
    public int? OrderId { get; set; }

    [Column("userId")]
    public int? UserId { get; set; }

    [Column("reason")]
    public string? Reason { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string? Status { get; set; }

    [Column("createdAt", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("ReturnRequests")]
    public virtual OrderTable? Order { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("ReturnRequests")]
    public virtual User? User { get; set; }
}
