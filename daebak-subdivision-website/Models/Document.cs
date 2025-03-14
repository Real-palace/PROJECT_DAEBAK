namespace daebak_subdivision_website.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; } // Path to stored document
        public DateTime UploadDate { get; set; }
        public int UserId { get; set; } // Assuming a relation to User
    }
}
