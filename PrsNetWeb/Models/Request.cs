﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PrsNetWeb.Models;

[Table("Request")]
public partial class Request
{
	[Key]
	[Column("ID")]
	public int Id { get; set; }

	[Column("UserID")]
	public int UserId { get; set; }

	[StringLength(50)]
	[Unicode(false)]
	public string RequestNumber { get; set; } = null!;

	[StringLength(100)]
	[Unicode(false)]
	public string Description { get; set; } = null!;

	[StringLength(255)]
	[Unicode(false)]
	public string Justification { get; set; } = null!;

	public DateTime DateNeeded { get; set; }

	[StringLength(25)]
	[Unicode(false)]
	public string DeliveryMode { get; set; } = null!;

	[StringLength(20)]
	[Unicode(false)]
	public string Status { get; set; } = null!;

	[Column(TypeName = "decimal(10, 2)")]
	public decimal Total { get; set; }

	[Column(TypeName = "datetime")]
	public DateTime SubmittedDate { get; set; }

	[StringLength(100)]
	[Unicode(false)]
	public string? ReasonForRejection { get; set; }

	[InverseProperty("Request")]
	public virtual ICollection<LineItem> LineItems { get; set; } = new List<LineItem>();

	//[ForeignKey("UserId")]
	//[InverseProperty("Requests")]
	public User? User { get; set; } = null!;
}