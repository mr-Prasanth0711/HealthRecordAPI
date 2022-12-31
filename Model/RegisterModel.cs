using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthRecordAPI.Model
{
    public class RegisterModel
    {
        [Key]   
        public string ?Username { get; set; }

        [EmailAddress]
        public string ?Email { get; set; }
        public string ?mobile { get; set; }
        public string ?Password { get; set; }
        public int ?OTP { get; set; }
        public string ?Token { get; set; }
        public string ?Role { get; set;}
        public DateTime? Changepassworddate { get; set; }
    }
}
