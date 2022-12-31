using System.ComponentModel.DataAnnotations;

namespace HealthRecordAPI.Model
{
    public class LoginModel
    {

        [Key]
        public string Username { get; set; } 

        public string Password { get; set; }

       
    }
}
