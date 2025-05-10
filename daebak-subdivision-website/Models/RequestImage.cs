using System;

namespace daebak_subdivision_website.Models
{
    public class RequestImage
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string ImagePath { get; set; }
        public DateTime UploadedAt { get; set; }
        
        // Navigation property
        public ServiceRequest ServiceRequest { get; set; }
    }
} 