using System.ComponentModel.DataAnnotations;

namespace HealthRecordAPI.Model
{
    public class AdminModel
    {
        [Key]
        public string username { get; set; }=string.Empty;

        public string password { get; set; }= string.Empty;

        public string ?Role { get; set; }

  
    }
}
