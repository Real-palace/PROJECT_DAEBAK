using System;

namespace daebak_subdivision_website.Models
{
    public class FeedbackViewModel
    {
        public int FeedbackId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string HouseNumber { get; set; } = string.Empty;
        public string FeedbackType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }
}
