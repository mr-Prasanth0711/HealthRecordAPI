using HealthRecordAPI.Context;
using HealthRecordAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HealthRecordAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasicdetailController : ControllerBase
    {
        private readonly AppDatabase _authdatabase;
        private readonly IConfiguration _configuration;
        private SqlConnection conn;


        public BasicdetailController(AppDatabase appDatabase, IConfiguration configuration)
        {
            _authdatabase = appDatabase;
            _configuration = configuration;

            conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnStr"));
        }

        [HttpPost("Basicdetail")]
       public async Task<IActionResult> BasicDetail([FromBody] BasicDetailModel DetailsObject)
        {
             
                if (DetailsObject == null)
                    return BadRequest();

                await _authdatabase.Details.AddAsync(DetailsObject);
                await _authdatabase.SaveChangesAsync();
                return Ok(DetailsObject);
             
            
        }
        [HttpGet("GetDetails")]
        public async Task<IActionResult>GetDetails()
        {
            var user = await _authdatabase.Details.ToListAsync();
            return Ok(user);
        } 
    }
}
