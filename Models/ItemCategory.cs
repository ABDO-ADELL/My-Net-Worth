using System.ComponentModel.DataAnnotations;

namespace PRISM.Models
{
    public class ItemCategory
    {
        [Key]
        public int ProductCategoryId { get; set; }
        public int BusinessId { get; set; }
        public string Name { get; set; } = string.Empty;

        public Business Business { get; set; } 

        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
