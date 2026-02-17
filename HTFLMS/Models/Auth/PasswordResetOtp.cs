using System;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models.Auth
{
    public class PasswordResetOtp
    {
        public int Id { get; set; }

        [Required]
        public int UserIdInt { get; set; }  // maps to User.Id

        [Required]
        public string Email { get; set; } = "";

        [Required]
        public string OtpHash { get; set; } = "";

        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public bool IsUsed { get; set; } = false;
        public bool IsVerified { get; set; } = false;

        public int Attempts { get; set; } = 0;

        // a public "flow id" so we don't expose internal Ids easily
        [Required]
        public string FlowId { get; set; } = Guid.NewGuid().ToString("N");
    }
}
