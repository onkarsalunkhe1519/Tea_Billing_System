using System;
using System.ComponentModel.DataAnnotations;

namespace Tea_Billing_System.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; }  // User email from session

        [Required]
        public string TeaName { get; set; }  // Tea product name

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";  // Default status

        public string? PaymentStatus { get; set; } = "Pending"; // Razorpay Payment Status
        public string? RazorpayPaymentId { get; set; } // Razorpay Payment ID

        public DateTime OrderDate { get; set; } = DateTime.Now;
    }
}
