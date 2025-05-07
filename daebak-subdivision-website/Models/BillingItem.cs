using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("BILLING_ITEMS")]
    public class BillingItem
    {
        [Key]
        [Column("ITEM_ID")]
        public int ItemId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("ITEM_NAME")]
        public string ItemName { get; set; }

        [Required]
        [Column("DESCRIPTION")]
        public string Description { get; set; }

        [Required]
        [Column("AMOUNT", TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column("DUE_DATE")]
        public DateTime DueDate { get; set; }

        [Column("IS_RECURRING")]
        public bool IsRecurring { get; set; } = false;

        [StringLength(50)]
        [Column("CATEGORY")]
        public string Category { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
