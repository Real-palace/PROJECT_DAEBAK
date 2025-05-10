using System;

namespace daebak_subdivision_website.Models
{
    public class FeedbackViewModel
    {
        public int FeedbackId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string HouseNumber { get; set; } // This property exists in the ViewModel but not in the Feedback model
        public string FeedbackType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string CreatedAt { get; set; }
    }
}
