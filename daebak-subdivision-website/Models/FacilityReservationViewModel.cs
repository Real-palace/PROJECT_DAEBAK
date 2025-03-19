using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using daebak_subdivision_website.Models;

namespace daebak_subdivision_website.ViewModels
{
    public class FacilityReservationViewModel
    {
        public int ReservationId { get; set; }
        
        [Required]
        public int FacilityId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Reservation Date")]
        [DataType(DataType.DateTime)]
        public DateTime ReservationDate { get; set; }

        [Required]
        [StringLength(10)]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Facility Details
        public string FacilityName { get; set; } // Ensure this property exists

        // User Details
        public string UserName { get; set; } // Change from UserFullName to UserName if necessary

        // Dropdown List for Facilities
        public IEnumerable<Facility> AvailableFacilities { get; set; }
    }
}
