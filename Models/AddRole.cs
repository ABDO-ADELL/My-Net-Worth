using System.ComponentModel.DataAnnotations;

namespace PRISM.Models
{
    public class AddRole
    {
        [Required]
        public string RoleName { get; set; }
        [Required]
        public string UserId { get; set; }


    }
}
