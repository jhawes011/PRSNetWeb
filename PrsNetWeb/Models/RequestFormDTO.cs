namespace PrsNetWeb.Models
{
	public class RequestFormDTO
	{
		public int UserId { get; set; } // Added by the frontend as (signed-in user)
		public string Description { get; set; }
		public string Justification { get; set; }
		public DateOnly DateNeeded { get; set; }
		public string DeliveryMode { get; set; }
		public string Status { get; set; }
		public int Total { get; set; }
		
	}
}
