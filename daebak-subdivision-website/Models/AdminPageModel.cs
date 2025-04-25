using System.Collections.Generic;

namespace daebak_subdivision_website.Models
{
    public class AdminPageModel
    {
        public string Announcement { get; set; } = "Stay tuned for updates and news.";
        public IEnumerable<Event> Events { get; set; } = new List<Event>();
    }
}
