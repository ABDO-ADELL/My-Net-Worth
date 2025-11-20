using PRISM.Models.Authmodels;
using System.ComponentModel.DataAnnotations.Schema;
namespace PRISM.Models
{
    public class Business
    {
        [Key]
        public int BusinessId { get; set; }
        public string Name { get; set; }
        public string Industry { get; set; }
        public string Timezone { get; set; }
        public string Status { get; set; }
        public bool IsDeleted { get; set; }

        // Relations
        public string ?UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser? Users { get; set; }
        public ICollection<Branch>? Branches { get; set; }
        public ICollection<Items>? Items { get; set; }
    }

}
