using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Models;

[Table("Dispute")]
public partial class Dispute
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("orderId")]
    public int? OrderId { get; set; }

    [Column("raisedBy")]
    public int? RaisedBy { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string? Status { get; set; }

    [Column("resolution")]
    public string? Resolution { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("Disputes")]
    public virtual OrderTable? Order { get; set; }

    [ForeignKey("RaisedBy")]
    [InverseProperty("Disputes")]
    public virtual User? RaisedByNavigation { get; set; }
}
