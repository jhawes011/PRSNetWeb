using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PrsNetWeb.Models;

[Table("LineItem")]
[Index("RequestId", "ProductId", Name = "req_pdt", IsUnique = true)]
public partial class LineItem
{
	[Key]
	[Column("ID")]
	public int Id { get; set; }

	[Column("RequestID")]
	[Required]
	public int RequestId { get; set; }

	[Column("ProductID")]
	[Required]
	public int ProductId { get; set; }

	[Required]
	[Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
	public int Quantity { get; set; }

	
	public Product? Product { get; set; } = null!;
	public Request? Request { get; set; } = null!;
}
