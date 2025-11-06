using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRISM.Models
{
    public class ItemCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "BusinessId is required")]
        [ForeignKey("Business")]
        public int BusinessId { get; set; }

        public bool IsArchived { get; set; } = false;

        // Navigation properties
        public Business? Business { get; set; }
        public ICollection<Items>? Items { get; set; }
    }
}
