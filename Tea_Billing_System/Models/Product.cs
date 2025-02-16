using System.ComponentModel.DataAnnotations;

namespace Tea_Billing_System.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tea name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }

        // Image stored as a URL (Can be uploaded)
        public string ImageUrl { get; set; }
    }
}
