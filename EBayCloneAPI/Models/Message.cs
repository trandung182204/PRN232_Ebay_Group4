using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Models;

[Table("Message")]
public partial class Message
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("senderId")]
    public int? SenderId { get; set; }

    [Column("receiverId")]
    public int? ReceiverId { get; set; }

    [Column("content")]
    public string? Content { get; set; }

    [Column("timestamp", TypeName = "datetime")]
    public DateTime? Timestamp { get; set; }

    [ForeignKey("ReceiverId")]
    [InverseProperty("MessageReceivers")]
    public virtual User? Receiver { get; set; }

    [ForeignKey("SenderId")]
    [InverseProperty("MessageSenders")]
    public virtual User? Sender { get; set; }
}
