using System.ComponentModel.DataAnnotations;

namespace PRISM.Models.Authmodels
{
    public class LoginModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, MinLength(6)]
        public string Password { get; set; }
    }
}
