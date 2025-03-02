﻿using System;
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
			var lineItems = _context.LineItems.Include(l => l.Product)
											 .Include(l => l.Request);
            return await lineItems.ToListAsync();
        }

        // GET: api/LineItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LineItem>> GetLineItem(int id)
        {
            var lineItem = await _context.LineItems.Include(l => l.Product)
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
			var lineItems = await _context.LineItems.Include(l => l.Product)
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
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
				RecalculateLineItemTotal(lineItem.RequestId);
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LineItem>> PostLineItem(LineItem lineItem)
        {
            _context.LineItems.Add(lineItem);
            await _context.SaveChangesAsync();
			RecalculateLineItemTotal(lineItem.RequestId);
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
			RecalculateLineItemTotal(lineItem.RequestId);
			return NoContent();
		}

        private bool LineItemExists(int id)
        {
            return _context.LineItems.Any(e => e.Id == id);
        }
		
		private void RecalculateLineItemTotal(int reqId)
		{
			var request = _context.Requests
								  .Include(r => r.LineItems)
								  .ThenInclude(li => li.Product)
								  .FirstOrDefault(r => r.Id == reqId);

			if (request == null) return;

			request.Total = request.LineItems.Sum(li => li.Quantity * (li.Product?.Price ?? 0));
			
			_context.SaveChanges();
		}

		
		
	}
}
