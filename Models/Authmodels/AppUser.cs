using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRISM.Models.Authmodels
{
    public class AppUser : IdentityUser
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; }
        [Required, MaxLength(50)]
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public ICollection<Business>? Business { get; set; }

        public List<RefreshTokens>? RefreshTokens { get; set; }

    }
}
