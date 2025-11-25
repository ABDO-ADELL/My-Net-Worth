using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRISM.Models
{
    public class Items
    {
        [Key]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "BusinessId is required.")]
        public int BusinessId { get; set; }

        [Required(ErrorMessage = "BranchId is required.")]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name can't exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "SKU is required.")]
        [StringLength(50, ErrorMessage = "SKU can't exceed 50 characters.")]
        public string Sku { get; set; }  // Stock Keeping Unit

        [Range(0, double.MaxValue, ErrorMessage = "CostPrice must be a non-negative value.")]
        public decimal CostPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "SellPrice must be a non-negative value.")]
        public decimal SellPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "DurationMinutes must be a positive number.")]
        public int? DurationMinutes { get; set; } // for services

        [StringLength(500, ErrorMessage = "Description can't exceed 500 characters.")]
        public string? Description { get; set; }

        public bool IsDeleted { get; set; } = false;

        // ✅ Relations (Navigation properties)
        [ForeignKey("BusinessId")]
        public Business? Business { get; set; }

        [ForeignKey("BranchId")]
        public Branch? Branch { get; set; }

        [ForeignKey("CategoryId")]
        [Required(ErrorMessage = "CategoryId is required.")]
        public int CategoryId { get; set; }

        public ItemCategory? ItemCategory { get; set; }

        public ICollection<Inventory>? Inventories { get; set; }
    }
}
