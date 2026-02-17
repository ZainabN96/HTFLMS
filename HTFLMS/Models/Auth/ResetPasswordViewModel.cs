using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models.Auth
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string FlowId { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = "";

        [Required, Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = "";
    }
}
