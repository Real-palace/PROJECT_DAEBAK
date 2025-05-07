using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("PAYMENTS")]
    public class Payment
    {
        [Key]
        [Column("PAYMENT_ID")]
        public int PaymentId { get; set; }
        
        [Column("BILL_ID")]
        public int? BillId { get; set; }
        
        [Column("USER_ID")]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(20)]
        [Column("RECEIPT_NUMBER")]
        public string ReceiptNumber { get; set; }
        
        [Required]
        [Column("PAYMENT_DATE")]
        public DateTime PaymentDate { get; set; }
        
        [Required]
        [Column("AMOUNT", TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(50)]
        [Column("PAYMENT_METHOD")]
        public string PaymentMethod { get; set; }
        
        [StringLength(100)]
        [Column("REFERENCE_NUMBER")]
        public string ReferenceNumber { get; set; }
        
        [Column("PAYMENT_DETAILS")]
        public string PaymentDetails { get; set; }
        
        [Column("PROOF_OF_PAYMENT_URL")]
        public string ProofOfPaymentUrl { get; set; }
        
        [Required]
        [StringLength(20)]
        [Column("STATUS")]
        public string Status { get; set; } = "Pending";
        
        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        [ForeignKey("BillId")]
        public virtual UserBill Bill { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
