using Microsoft.EntityFrameworkCore;

namespace PRISM.Models.Authmodels
{
    [Owned]
    public class RefreshTokens
    {
        public string? Token { get; set; }
        public DateTime ExpiresOn { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
        public DateTime CreatedOn { get; set; }

        public DateTime? RevokeOn { get; set; }
        public bool IsActive => RevokeOn == null &&!IsExpired;


    }
}
