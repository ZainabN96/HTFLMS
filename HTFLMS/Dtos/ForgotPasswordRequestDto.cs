using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Dtos
{
    public class ForgotPasswordRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
    }
}