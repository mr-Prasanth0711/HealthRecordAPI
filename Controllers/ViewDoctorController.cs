using HealthRecordAPI.Context;
using HealthRecordAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;

namespace HealthRecordAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViewDoctorController : ControllerBase
    {
        private readonly AppDatabase _authdatabase;
        private readonly IConfiguration _configuration;
        private SqlConnection conn;
        string email, username;
        int otp;

        public ViewDoctorController(AppDatabase appDatabase, IConfiguration configuration)
        {
            _authdatabase = appDatabase;
            _configuration = configuration;

            conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnStr"));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> doctorDetail([FromRoute] int id)
        {
            try
            {
                var Userinformation = from var in _authdatabase.Record where var.Id == id select var;
                if (Userinformation != null)
                {
                    return Ok(Userinformation);
                }
                else
                {
                    return Ok("User information not found");
                }
            }
            catch
            {
                return BadRequest("Somthing went wrong");
            }
        }
        [HttpPost("DoctorEdit")]
        public async Task<IActionResult> DoctorEditAPI([FromBody] RecordModel Edit)
        {
            var user = await _authdatabase.Record.FirstOrDefaultAsync(x => x.Id == Edit.Id);
            if (user != null)
            {
                Edit.files = Edit.files.Remove(0, 12);

                user.Name = Edit.Name;
                user.DoctorName = Edit.DoctorName;
                user.RecordType = Edit.RecordType;
                user.Email = Edit.Email;
                user.Problem = Edit.Problem;
                user.Address = Edit.Address;
                user.LastDate = Edit.LastDate;
                user.NextDate = Edit.NextDate;
                user.files = Edit.files;

                await _authdatabase.SaveChangesAsync();
                return Ok(new { message = "Update Saved Successfully" });

            }
            else
            {
                return BadRequest(new { message = "Please Fill All Fields" });
            }
        }




    }
}
