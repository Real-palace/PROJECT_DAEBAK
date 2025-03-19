namespace daebak_subdivision_website.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // Add this property to hold error messages
        public string? ErrorMessage { get; set; }
    }
}
