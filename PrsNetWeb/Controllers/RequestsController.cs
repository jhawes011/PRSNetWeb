using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PrsNetWeb.Models;

namespace PrsNetWeb.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RequestsController : ControllerBase
	{
		private readonly PRSDBContext _context;
		public RequestsController(PRSDBContext context)
		{
			_context = context;
		}

		// GET: api/Requests
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Request>>> GetRequests()
		{
			var requests = _context.Requests.Include(r => r.User);
			return await requests.ToListAsync();
		}
		// GET: api/Requests/5
		[HttpGet("{id}")]
		public async Task<ActionResult<Request>> GetRequest(int id)
		{
			var request = await _context.Requests.Include(r => r.User)
												 .FirstOrDefaultAsync(r => r.Id == id);
			if (request == null)
			{
				return NotFound();
			}

			return request;
		}
		// PUT: api/Requests/5
		[HttpPut("{id}")]
		public async Task<IActionResult> PutRequest(int id, Request request)
		{
			if (id != request.Id)
			{
				return BadRequest();
			}

			_context.Entry(request).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!RequestExists(id))
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
		// DELETE: api/Requests/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteRequest(int id)
		{
			//var request = await _context.Requests.FindAsync(id);
			var request = await _context.Requests.Include(r => r.LineItems).FirstOrDefaultAsync(r => r.Id == id);
			if (request == null)
			{
				return NotFound();
			}
			_context.LineItems.RemoveRange(request.LineItems);
			_context.Requests.Remove(request);
			await _context.SaveChangesAsync();
			return NoContent();
		}
		// POST
		[HttpPost]
		public async Task<ActionResult<Request>> CreateRequest(Request request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var newRequest = new Request
			{
				UserId = request.UserId,
				Description = request.Description,
				Justification = request.Justification,
				DateNeeded = request.DateNeeded,
				DeliveryMode = request.DeliveryMode,
				RequestNumber = await GenerateRequestNumber(),
				Status = "NEW",
				Total = 0.0m,
				SubmittedDate = DateTime.UtcNow
			};
			nullifyAndSetId(request);
			_context.Requests.Add(newRequest);
			await _context.SaveChangesAsync();

			return CreatedAtAction("GetRequest", new { id = newRequest.Id }, newRequest);
		}
		private void nullifyAndSetId(Request request)
		{
			Console.WriteLine("Req Nullify: Req: " + request.ToString());
			if (request != null)
			{
				if (request.Id == 0)
				{
					request.UserId = request.User.Id;
				}
				request = null;
			}
			
		}


		// POST /api/requests/submit-review/{id}
		[HttpPut("submit-review/{id}")]
		public async Task<IActionResult> SubmitRequestForReview(int id)
		{
			var request = await _context.Requests.FindAsync(id);
			if (request == null)
			{
				return NotFound();
			}

			request.Status = request.Total <= 50? "APPROVED" : "REVIEW";
			request.SubmittedDate = DateTime.Now;

			_context.Entry(request).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!RequestExists(id))
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
		// GET: api/Requests/list-review/{userId}
		[HttpGet("list-review/{userId}")]
		public async Task<ActionResult<List<Request>>> ListReview(int userId)
		{
			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				return NotFound("User not found.");
			}
			if (!user.Reviewer)
			{
				return NotFound("Review access denied.");
			}
			var requests = await _context.Requests
				.Where(r => r.UserId != userId && r.Status == "REVIEW") 
				
				.ToListAsync();

			if (requests.Count == 0)
			{
				return NotFound("No requests in REVIEW.");
			}

			return Ok(requests);
		}
		//PUT Review Approve
		[HttpPut("approve/{id}")]
		public async Task<IActionResult> ApproveRequest(int id)
		{
			var request = await _context.Requests.FindAsync(id);
			if (request == null)
			{
				return NotFound();
			}


			request.Status = "APPROVED";

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!RequestExists(id))
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
		//PUT review reject
		[HttpPut("reject/{id}")]
		public async Task<IActionResult> RejectRequest(int id, [FromBody] RequestDenyDTO requestDenyDTO)
		{
			var request = await _context.Requests.FindAsync(id);
			if (request == null)
			{
				return NotFound();
			}

			request.Status = "REJECTED";
			request.ReasonForRejection = requestDenyDTO.ReasonForRejection;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!RequestExists(id))
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

		private async Task<string> GenerateRequestNumber()
		{
			string dateSection = DateTime.UtcNow.ToString("yyMMdd");
			string prefix = "R";
			int nextRequestNum = 1;
			var newestRequest = await _context.Requests
											  .OrderByDescending(r => r.RequestNumber)
											  .FirstOrDefaultAsync();
			if (newestRequest != null)
			{
				string newestRequestNumStr = newestRequest.RequestNumber.Substring(7, 4);
				if (int.TryParse(newestRequestNumStr, out int newestRequestNum))
				{
					nextRequestNum = newestRequestNum + 1;
				}
			}
			string requestNumber = $"{prefix}{dateSection}{nextRequestNum:D4}";
			return requestNumber;
		}
		private bool RequestExists(int id)
		{
			return _context.Requests.Any(e => e.Id == id);
		}
		
	}
}
