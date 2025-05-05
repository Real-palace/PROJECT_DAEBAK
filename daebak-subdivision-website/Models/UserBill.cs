using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("USER_BILLS")]
    public class UserBill
    {
        [Key]
        [Column("BILL_ID")]
        public int BillId { get; set; }
        
        [Column("USER_ID")]
        public int UserId { get; set; }
        
        [Column("ITEM_ID")]
        public int ItemId { get; set; }
        
        [Required]
        [StringLength(20)]
        [Column("BILL_NUMBER")]
        public string BillNumber { get; set; }
        
        [Required]
        [Column("AMOUNT", TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [Column("DUE_DATE")]
        public DateTime DueDate { get; set; }
        
        [Required]
        [StringLength(20)]
        [Column("STATUS")]
        public string Status { get; set; } = "Pending";
        
        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        [ForeignKey("ItemId")]
        public virtual BillingItem BillingItem { get; set; }
    }
}
