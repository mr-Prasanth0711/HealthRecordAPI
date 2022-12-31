using System.ComponentModel.DataAnnotations;

namespace HealthRecordAPI.Model
{
    public class userdetails
    {
        [Key]
        public string Username { get; set; }
    }
}
