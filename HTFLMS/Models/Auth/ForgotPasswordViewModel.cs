using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models.Auth
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
    }
}
