using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrsNetWeb.Models;

namespace PrsNetWeb.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LineItemsController : ControllerBase
	{
		private readonly PRSDBContext _context;

		public LineItemsController(PRSDBContext context)
		{
			_context = context;
		}

		// GET: api/LineItems
		[HttpGet]
		public async Task<ActionResult<IEnumerable<LineItem>>> GetLineItems()
		{
			var lineItems = await _context.LineItems
										  .Include(l => l.Product)
										  .ThenInclude(p => p.Vendor) // Eagerly load the Vendor property
										  .Include(l => l.Request)
										  .ToListAsync();
			return lineItems;
		}

		// GET: api/LineItems/5
		[HttpGet("{id}")]
		public async Task<ActionResult<LineItem>> GetLineItem(int id)
		{
			var lineItem = await _context.LineItems
										 .Include(l => l.Product)
										 .ThenInclude(p => p.Vendor) // Eagerly load the Vendor property
										 .Include(l => l.Request)
										 .FirstOrDefaultAsync(l => l.Id == id);

			if (lineItem == null)
			{
				return NotFound();
			}

			return lineItem;
		}

		// GET: api/LineItems/lines-for-req/{reqID}
		[HttpGet("lines-for-req/{reqID}")]
		public async Task<ActionResult<IEnumerable<LineItem>>> GetLineItemsByRequestId(int reqID)
		{
			var lineItems = await _context.LineItems
										  .Include(l => l.Product)
										  .ThenInclude(p => p.Vendor) // Eagerly load the Vendor property
										  .Include(l => l.Request)
										  .Where(l => l.RequestId == reqID)
										  .ToListAsync();

			if (lineItems == null || !lineItems.Any())
			{
				return NotFound();
			}

			return lineItems;
		}

		// PUT: api/LineItems/5
		[HttpPut("{id}")]
		public async Task<IActionResult> PutLineItem(int id, LineItem lineItem)
		{
			if (id != lineItem.Id)
			{
				return BadRequest();
			}

			_context.Entry(lineItem).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
				await RecalculateLineItemTotal(lineItem.RequestId);
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!LineItemExists(id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			return NoContent();
		}

		// POST: api/LineItems
		[HttpPost]
		public async Task<ActionResult<LineItem>> PostLineItem(LineItem lineItem)
		{
			

			nullifyAndSetId(lineItem);
			_context.LineItems.Add(lineItem);
			await _context.SaveChangesAsync();
			bool success = await RecalculateLineItemTotal(lineItem.RequestId);
			if (!success)
			{
				return Problem($"Error recalculating request total for request id: {lineItem.RequestId}");
			}

			return CreatedAtAction("GetLineItem", new { id = lineItem.Id }, lineItem);
		}

		// DELETE: api/LineItems/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteLineItem(int id)
		{
			var lineItem = await _context.LineItems.FindAsync(id);
			if (lineItem == null)
			{
				return NotFound();
			}

			_context.LineItems.Remove(lineItem);
			await _context.SaveChangesAsync();
			await RecalculateLineItemTotal(lineItem.RequestId);
			return NoContent();
		}

		private bool LineItemExists(int id)
		{
			return _context.LineItems.Any(e => e.Id == id);
		}

		private async Task<bool> RecalculateLineItemTotal(int reqId)
		{
			var request = await _context.Requests
			  .Include(r => r.LineItems)
			  .ThenInclude(li => li.Product)
			  .FirstOrDefaultAsync(r => r.Id == reqId);

			if (request == null) return false;

			request.Total = request.LineItems.Sum(li => li.Quantity * (li.Product?.Price ?? 0));

			await _context.SaveChangesAsync();
			return true;
		}

		private void nullifyAndSetId(LineItem lineItem)
		{
			Console.WriteLine("LI Nullify: LI: " + lineItem.ToString());
			if (lineItem.Request != null)
			{
				if (lineItem.RequestId == 0)
				{
					lineItem.RequestId = lineItem.Request.Id;
				}
				lineItem.Request = null;
			}
			if (lineItem.Product != null)
			{
				if (lineItem.ProductId == 0)
				{
					lineItem.ProductId = lineItem.Product.Id;
				}
				lineItem.Product = null;
			}
		}
	}
}