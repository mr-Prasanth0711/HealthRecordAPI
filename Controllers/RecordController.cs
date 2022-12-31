using HealthRecordAPI.Context;
using HealthRecordAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace HealthRecordAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecordController : ControllerBase
    {

        private readonly AppDatabase _authdatabase;
        private readonly IConfiguration _configuration;
        private SqlConnection conn;


        public RecordController(AppDatabase appDatabase, IConfiguration configuration)
        {
            _authdatabase = appDatabase;
            _configuration = configuration;

            conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnStr"));
        }


        [HttpPost("Records")]
        public async Task<IActionResult> UserRecords([FromBody] RecordModel recordObject)
        {

            if (recordObject == null)
                return BadRequest();

            await _authdatabase.Record.AddAsync(recordObject);
            await _authdatabase.SaveChangesAsync();
            return Ok(new { message = "Saved Successfullyyyy" }); 
        }


        [HttpGet]
        [Route("{Username}")]
        public async Task<IActionResult> UserDetails([FromRoute] string Username)
        {
            try
            {
                var UserRecord = from var in _authdatabase.Record where var.Name == Username select var;
                if (UserRecord != null)
                {
                    return Ok(UserRecord);
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
        [HttpPost]
        [Route("Uploadfile")]
        public async Task<IActionResult> Uploadfile(IFormFile file)
        {
            try
            {
                var output = SaveFile(file);
                return Ok(new { Message = "Successfully Uploaded" });
            }
            catch
            {
                return BadRequest();
            }
        }
        public static string SaveFile(IFormFile formFile)
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory() + @"\ImportFiles\UploadedFiles");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var ext = Path.GetExtension(formFile.FileName);
                var allowedExtensions = new string[] { ".doc", ".xls", ".ppt", ".pdf", ".jpg" };
                if (!allowedExtensions.Contains(ext))
                {
                    string msg = string.Format("Only{0}extension are allowed", string.Join(",", allowedExtensions));
                    return msg;
                }
                string uniqueString = formFile.FileName;
                //we are try to create Unique filename here
                var newFileName = uniqueString + ext;
                var filewithPath = Path.Combine(path, newFileName);
                var stream = new FileStream(filewithPath, FileMode.Create);
                formFile.CopyTo(stream);
                stream.Close();
                return newFileName;
            }
            catch (Exception ex)
            {
                return "Error has Occured";
            }
        }






    }
}
