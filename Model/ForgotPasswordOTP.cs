using System.ComponentModel.DataAnnotations;

namespace HealthRecordAPI.Model
{
    public class ForgotPasswordOTP
    {
        [Key]

        [Required]
        [EmailAddress]
        public string Email { get; set; } 
    }
}
