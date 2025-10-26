using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

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
     
        public List<RefreshTokens>? RefreshTokens { get; set; }

    }
}
