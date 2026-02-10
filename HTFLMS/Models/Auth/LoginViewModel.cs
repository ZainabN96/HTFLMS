using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models.Auth
{
    public class LoginViewModel
    {
        [Required]
        public string UserId { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
