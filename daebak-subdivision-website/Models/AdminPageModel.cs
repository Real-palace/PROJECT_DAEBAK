using System.Collections.Generic;

namespace daebak_subdivision_website.Models
{
    public class AdminPageModel
    {
        public string Announcement { get; set; } = "Stay tuned for updates and news.";
        public IEnumerable<Event> Events { get; set; } = new List<Event>();

        // Dashboard statistics properties
        public int? UserCount { get; set; }
        public decimal? PaymentsReceived { get; set; }
        public int? ReservationsCount { get; set; }
        public int? ServiceRequestsCount { get; set; }
    }
}
