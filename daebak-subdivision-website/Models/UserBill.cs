namespace daebak_subdivision_website.Models
{
    public class UserBill
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BillingItemId { get; set; }
        public decimal AmountDue { get; set; }
        public DateTime DueDate { get; set; }
    }
}
