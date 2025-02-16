using System;
using System.ComponentModel.DataAnnotations;

namespace Tea_Billing_System.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; }  // User's email

        [Required]
        public string TeaName { get; set; }  // Tea being reviewed

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }  // Star rating (1 to 5)

        [Required]
        [MaxLength(500)]
        public string Comment { get; set; }  // User's comment

        public DateTime ReviewDate { get; set; } = DateTime.Now;
    }
}
