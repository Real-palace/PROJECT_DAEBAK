namespace daebak_subdivision_website.Models
{
    public class FeedbackStatusUpdateModel
    {
        public int Id { get; set; }
        public string Status { get; set; }
    }

    public class FeedbackDeleteModel
    {
        public int Id { get; set; }
    }

    public class FeedbackResponseModel
    {
        public int FeedbackId { get; set; }
        public string ResponseText { get; set; }
    }
}
