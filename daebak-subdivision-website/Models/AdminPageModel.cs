using System.Collections.Generic;
using System;

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
        
        // Billing page properties
        public IEnumerable<BillViewModel> Bills { get; set; } = new List<BillViewModel>();
        public IEnumerable<BillViewModel> PendingBills { get; set; } = new List<BillViewModel>();
        public IEnumerable<PaymentViewModel> PaymentRecords { get; set; } = new List<PaymentViewModel>();
    }
    
    // View models for billing data
    public class BillViewModel
    {
        public int BillId { get; set; }
        public string BillNumber { get; set; }
        public string HomeownerName { get; set; }
        public string HouseNumber { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
    }
    
    public class PaymentViewModel
    {
        public int PaymentId { get; set; }
        public string ReceiptNumber { get; set; }
        public string BillNumber { get; set; }
        public string HomeownerName { get; set; }
        public string HouseNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
    }
}
