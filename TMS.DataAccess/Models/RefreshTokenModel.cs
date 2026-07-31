using System;
namespace TMS.DataAccess.Models
{
    public class RefreshTokenModel
    {
        public int RefreshTokenId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string MobileNumber { get; set; }
        public string EmailId { get; set; }
        public string RoleName { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
