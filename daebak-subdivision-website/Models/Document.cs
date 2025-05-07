using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace daebak_subdivision_website.Models
{
    [Table("DOCUMENTS")]
    public class Document
    {
        [Key]
        [Column("DOCUMENT_ID")]
        public int DocumentId { get; set; }
        
        [Required]
        [Column("TITLE")]
        [StringLength(100)]
        public string Title { get; set; }
        
        [Column("FILE_PATH")]
        [StringLength(255)]
        public string FilePath { get; set; }
        
        [Column("CREATED_BY")]
        public int? CreatedById { get; set; }
        
        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; }
        
        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; }
        
        // Navigation property
        [ForeignKey("CreatedById")]
        public virtual User CreatedBy { get; set; }
        
        // Non-mapped properties for view models
        [NotMapped]
        public string DocumentType => System.IO.Path.GetExtension(FilePath)?.ToLowerInvariant() switch
        {
            ".pdf" => "PDF Document",
            ".doc" or ".docx" => "Word Document",
            ".xls" or ".xlsx" => "Excel Spreadsheet",
            ".png" or ".jpg" or ".jpeg" => "Image",
            _ => "Document"
        };
        
        [NotMapped]
        public string FormattedFileSize { get; set; }
        
        [NotMapped]
        public IFormFile DocumentFile { get; set; }
    }
}
