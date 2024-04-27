using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string Category { get; set; }
        [Required(ErrorMessage = "Code is required.")]
        [RegularExpression(@"P[0-9]{6}", ErrorMessage = "Code format must be PXXXXXX.")]
        public required string Code { get; set; }
        [Required(ErrorMessage = "Name field is required.")]
        [StringLength(maximumLength: 100, MinimumLength = 2)]
        public required string Name { get; set; }
        [Required, Url]
        public required string Image { get; set; }
        [Required]
        public required decimal Price { get; set; }
        [Required]
        public required int MinimumQuantity { get; set; }
        public decimal? DiscountRate { get; set; }
        [ForeignKey("User")]
        public Nullable<int> User { get; set; }
    }
}
