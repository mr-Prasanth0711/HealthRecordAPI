using System.ComponentModel.DataAnnotations;

namespace HealthRecordAPI.Model
{
    public class ConfirmForgetPasswordOTP
    {
        [Key]
        public int OTP { get; set; }
        public string NewPassword { get; set; }



    }
}
