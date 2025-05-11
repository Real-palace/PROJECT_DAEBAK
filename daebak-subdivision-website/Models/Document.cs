using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using System.IO;

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
        
        [Column("FILE_SIZE")]
        public long FileSize { get; set; }
        
        [Column("CREATED_BY")]
        public int? CreatedById { get; set; }
        
        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; }
        
        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; }
        
        [Column("CATEGORY")]
        [StringLength(50)]
        public string Category { get; set; }
        
        [Column("DESCRIPTION")]
        public string Description { get; set; }
        
        [Column("IS_PUBLIC")]
        public bool IsPublic { get; set; } = true; // Default to true so existing documents are visible
        
        // Navigation property
        [ForeignKey("CreatedById")]
        public virtual User CreatedBy { get; set; }
        
        // Non-mapped properties for view models
        [NotMapped]
        public IFormFile DocumentFile { get; set; }
        
        [NotMapped]
        public string DocumentType => Category;
        
        [NotMapped]
        public string FormattedFileSize
        {
            get
            {
                long bytes = FileSize;
                
                // Format file size
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                int order = 0;
                double len = bytes;
                
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                
                return $"{len:0.##} {sizes[order]}";
            }
        }
    }
}
