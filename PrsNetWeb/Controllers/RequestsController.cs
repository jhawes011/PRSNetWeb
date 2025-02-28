using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }
		
		[HttpPost]
		public async Task<IActionResult> CreateRequest(RequestForm requestForm)
		{
			if (requestForm == null)
			{
				return BadRequest("Invalid request data.");
			}

			var newRequest = new Request
			{
				UserId = requestForm.UserId, //********HELP Assigned from signed-in user
				Description = requestForm.Description,
				Justification = requestForm.Justification,
				DateNeeded = requestForm.DateNeeded,
				DeliveryMode = requestForm.DeliveryMode,

				                                 // Backend completed fields
				RequestNumber = await GenerateRequestNumber(),
				Status = "NEW",
				Total = 0.0m,
				SubmittedDate = DateTime.UtcNow
			};

			_context.Requests.Add(newRequest);
			await _context.SaveChangesAsync();

			return Ok(new
			{
				message = "---Request created---",
				requestId = newRequest.Id,
				requestNumber = newRequest.RequestNumber
			});
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
