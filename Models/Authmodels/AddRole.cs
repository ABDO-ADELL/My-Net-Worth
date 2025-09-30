using System.ComponentModel.DataAnnotations;

namespace PRISM.Models.Authmodels
{
    public class AddRole
    {
        [Required]
        public string RoleName { get; set; }
        [Required]
        public string UserId { get; set; }


    }
}
