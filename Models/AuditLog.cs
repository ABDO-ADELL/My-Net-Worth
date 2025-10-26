using PRISM.Models.Authmodels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRISM.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditId { get; set; }
        public int BusinessId { get; set; }
        public string UserId { get; set; }

        [Required]
        public string Action { get; set; }

        [Required]
        public string ObjectType { get; set; } 

        public int ObjectId { get; set; }  

        [Required]
        public DateTime Timestamp { get; set; }


        [ForeignKey(nameof(BusinessId))]
        public virtual Business Business { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual AppUser User { get; set; }
    }
}
