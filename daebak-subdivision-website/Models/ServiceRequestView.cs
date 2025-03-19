using System;
using System.ComponentModel.DataAnnotations;

namespace daebak_subdivision_website.Models
{
    public class ServiceRequestView
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Display(Name = "House Number")]
        public string? HouseNumber { get; set; }

        [Display(Name = "Request Type")]
        [Required]
        public string RequestType { get; set; }

        [Display(Name = "Description")]
        [Required]
        public string Description { get; set; }

        [Display(Name = "Status")]
        [Required]
        public string Status { get; set; }

        [Display(Name = "Assigned To")]
        public int? AssignedTo { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; }

        //Add ClosedAt to track request closure date
        [Display(Name = "Closed At")]
        public DateTime? ClosedAt { get; set; }

        public string? RequestedBy { get; set; }  // Full name of the user who made the request
        public string? AssignedToName { get; set; }  // Full name of the assigned person
    }
}
