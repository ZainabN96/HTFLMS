using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Dtos
{
    public class VerifyOtpRequestDto
    {
        [Required]
        public string FlowId { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be 6 digits.")]
        public string Otp { get; set; } = "";
    }
}