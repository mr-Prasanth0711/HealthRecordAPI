using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;

namespace HealthRecordAPI.Model
{
    public class BasicDetailModel
    {
        [Key]
        public int ?id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public DateTime dateofbirth { get; set; }
        public string age { get; set; }
        public string gender { get; set; }
        public string bloodgroup { get; set; }
        public string height { get; set; }
        public string weight { get; set; }
        public string bloodpressure { get; set; }

        public string diseases { get; set; }
        public string email { get; set; }
        public string street { get; set; }
        public string landmark { get; set; }
        public string city { get; set; }
        public string pincode { get; set; }
        public string mobileno { get; set; }


    }
}
