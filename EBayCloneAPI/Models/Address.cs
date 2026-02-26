using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Models;

[Table("Address")]
public partial class Address
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("userId")]
    public int? UserId { get; set; }

    [Column("fullName")]
    [StringLength(100)]
    public string? FullName { get; set; }

    [Column("phone")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Column("street")]
    [StringLength(100)]
    public string? Street { get; set; }

    [Column("city")]
    [StringLength(50)]
    public string? City { get; set; }

    [Column("state")]
    [StringLength(50)]
    public string? State { get; set; }

    [Column("country")]
    [StringLength(50)]
    public string? Country { get; set; }

    [Column("isDefault")]
    public bool? IsDefault { get; set; }

    [InverseProperty("Address")]
    public virtual ICollection<OrderTable> OrderTables { get; set; } = new List<OrderTable>();

    [ForeignKey("UserId")]
    [InverseProperty("Addresses")]
    public virtual User? User { get; set; }
}
