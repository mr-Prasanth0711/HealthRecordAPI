using System.ComponentModel.DataAnnotations;

namespace HealthRecordAPI.Model
{
    public class RecordModel
    {
        [Key]

        public int ?Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }
        public string RecordType { get; set; }

        public string DoctorName { get; set; }

        public string Problem { get; set; }

        public string Address { get; set; }

        public DateTime LastDate { get; set; }

        public DateTime NextDate { get; set; }

        public string ?files { get; set; }







    }
}




