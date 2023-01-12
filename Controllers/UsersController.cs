using HealthRecordAPI.Context;
using HealthRecordAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using HealthRecordAPI.Help;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using HealthRecordAPI.Migrations;
using System.Text;
using System.IO;

namespace HealthRecordAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDatabase _authdatabase;
        private readonly IConfiguration _configuration;
        private SqlConnection conn;
        string email, username;
        int otp;

        public UsersController(AppDatabase appDatabase, IConfiguration configuration)
        {
            _authdatabase = appDatabase;
            _configuration = configuration;

            conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnStr"));
        }


        [HttpPost("userlogin")]
        public async Task<IActionResult> Userlogin([FromBody] LoginModel loginObject)
        {
            if (loginObject == null)
                return BadRequest();
            var user = await _authdatabase.Users.FirstOrDefaultAsync(x => x.Username == loginObject.Username);

            if (user == null)
                return Ok(new { Message = "User not found." });

            if (!PasswordEncrypt.VerifyPassword(loginObject.Password, user.Password))
            {
                return BadRequest(new { Message = "Password is incorrect" });
            }

            user.Token = jwtToken(user);
            return Ok(new
            {
                Token = user.Token,
                message = user.Username
            });
        }

        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetDetails()
        {
            var user = from var in _authdatabase.Users where var.Role == "User" select var;

            return Ok(user);
        }

        [HttpPost("adminlogin")]
        public async Task<IActionResult> AdminLogin([FromBody] LoginModel adminloginObject)
        {
            if (adminloginObject == null)
                return BadRequest();
            var user = await _authdatabase.Users.FirstOrDefaultAsync(x => x.Username == adminloginObject.Username && x.Password == adminloginObject.Password && x.Role == "Admin");

            if (user == null)
                return Ok(new { Message = "User not found." });
            user.Token = jwtToken(user);
            return Ok(new
            {
                Token = user.Token,
                Message = "Login Successfully."
            });
        }



        private string jwtToken(RegisterModel user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryusefulforSecretKey.....");
            var Identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name,$"{user.Username}")
            });

            var Credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokendescriptor = new SecurityTokenDescriptor
            {
                Subject = Identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = Credentials,
            };
            var token = jwtTokenHandler.CreateToken(tokendescriptor);
            return jwtTokenHandler.WriteToken(token);
        }




        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterModel userObject)
        {
            if (userObject == null)
                return BadRequest();
            //Check Usename Exist
            if (await CheckUsenameExistAsync(userObject.Username))
                return BadRequest(new { message = "Username Already Exist" });
            //check email exist
            if (await CheckEmailExistAsync(userObject.Email))
                return BadRequest(new { message = "Email id Already Exist" });

            userObject.Password = PasswordEncrypt.PasswordEncryption(userObject.Password);
            userObject.Role = "User";
            userObject.Token = "";
            await _authdatabase.Users.AddAsync(userObject);

            await _authdatabase.SaveChangesAsync();
            return Ok(new { Message = "Registration Success." });
        }



        private Task<bool> CheckUsenameExistAsync(string username)
            => _authdatabase.Users.AnyAsync(x => x.Username == username);
        private Task<bool> CheckEmailExistAsync(string email)
           => _authdatabase.Users.AnyAsync(x => x.Email == email);


        public static bool ValidateEmail(string emailaddress)
        {
            string[] SplitEmail = emailaddress.Split(';');
            for (int i = 0; i < SplitEmail.Length; i++)
            {
                try
                {
                    Regex regex = new Regex(@"^((\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)*([;])*)*$");
                    Match match = regex.Match(SplitEmail[i]);
                    if (!match.Success)
                        return false;
                }
                catch (FormatException)
                {
                    return false;
                }
            }
            return true;
        }

        [HttpPost("sendforgetpasswordOTP")]
        public async Task<IActionResult> SendForgetPasswordOTP([FromBody] ForgotPasswordOTP forgotPasswordOTP)
        {
            if (forgotPasswordOTP == null)
                return BadRequest();
            var user = await _authdatabase.Users.FirstOrDefaultAsync(x => x.Email == forgotPasswordOTP.Email);

            var emailvalidation = ValidateEmail(forgotPasswordOTP.Email);
            if (emailvalidation == true)
            {
                if (user == null)
                {
                    return Ok(new { Message = "User not found." });
                }
                else
                {
                    var otp = GenerateRandomNo();

                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE users SET OTP = '" + otp + "' Where Username = '" + user.Username + "'", conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    var emailresult = SendOtpEmail(forgotPasswordOTP.Email, user.Username, otp);
                    if (emailresult == true)
                    {
                        return Ok(new { message = "Email Success" });
                        //mail  otp input 
                    }
                    else
                    {
                        return Ok(new { message = "Email Verified Successfully and OTP and sent to your email" });
                    }
                }
            }
            else
            {
                return Ok(new { message = "Please Enter Valid Email" });
            }
        }

        public static int GenerateRandomNo()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }

        public static bool SendOtpEmail(string Email, string username, int otp)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("mr.prasanth0711@gmail.com");
                    mail.To.Add(Email);
                    mail.Subject = "OTP - Forget Password for " + username + "";
                    mail.Body = "OTP : " + otp + "";
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("mr.prasanth0711@gmail.com", "Prasanth328@");
                        smtp.EnableSsl = true;
                        smtp.UseDefaultCredentials = true;
                        smtp.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        [HttpPost("confirmforgetpasswordOTP")]
        public async Task<IActionResult> ConfirmForgetPasswordOTP([FromBody] ConfirmForgetPasswordOTP confirmForgetPasswordOTP)
        {
            if (confirmForgetPasswordOTP == null)
                return BadRequest();
            var otp = await _authdatabase.Users.FirstOrDefaultAsync(x => x.OTP == confirmForgetPasswordOTP.OTP);

            if (otp == null)
            {
                return NotFound(new { Message = "Invalid OTP" });
            }
            else
            {
                if (confirmForgetPasswordOTP.OTP == otp.OTP)
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE users SET ChangePasswordDate = getdate() , Password = '" + confirmForgetPasswordOTP.NewPassword + "' Where UserName = '" + otp.Username + "'", conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    return Ok(new { message = "Password Successfully changed" });
                }
                else
                {
                    return Ok(new { message = "OTP Verification Failed" });
                }
            }
        }


        [HttpGet]
        [Route("{Username}")]
        public async Task<IActionResult> UserDetails([FromRoute] string Username)
        {
            try
            {
                var Userinformation = from var in _authdatabase.Users where var.Username == Username select var;
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



        [HttpGet]
        [Route("Details/{firstname}")]
        public async Task<IActionResult> BasicDetailget([FromRoute] string firstname)
        {
            try
            {
                var UserBasicDetails = from var in _authdatabase.Details where var.firstname == firstname select var;
                if (UserBasicDetails != null)
                {
                    return Ok(UserBasicDetails);
                }
                else
                {
                    return Ok("User information not found");
                }
            }
            catch
            {
                return BadRequest("Somthing went wroung");
            }
        }



        [HttpPost("SaveEdit")]
        public async Task<IActionResult> EditAPI([FromBody] BasicDetailModel Edit)
        {
            var user = await _authdatabase.Details.FirstOrDefaultAsync(x => x.firstname == Edit.firstname);
            if (user != null)
            {
                user.firstname = Edit.firstname;
                user.lastname = Edit.lastname;
                user.dateofbirth = Edit.dateofbirth;
                user.age = Edit.age;
                user.gender = Edit.gender;
                user.bloodgroup = Edit.bloodgroup;
                user.height = Edit.height;
                user.weight = Edit.weight;
                user.bloodpressure = Edit.bloodpressure;
                user.diseases = Edit.diseases;
                user.email = Edit.email;
                user.street = Edit.street;
                user.landmark = Edit.landmark;
                user.city = Edit.city;
                user.pincode = Edit.pincode;
                user.mobileno = Edit.mobileno;
                await _authdatabase.SaveChangesAsync();
                return Ok(new { message = "Update Saved Successfully" });

            }
            else
            {
                return BadRequest(new { message = "Please Fill All Fields" });
            }
        }


        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword newchangePassword)
        {
            var user = await _authdatabase.Users.FirstOrDefaultAsync(x => x.Username == newchangePassword.username);

            if (PasswordEncrypt.VerifyPassword(newchangePassword.oldpassword, user.Password))
            {
                if (newchangePassword.newpassword == newchangePassword.confirmpassword)
                {
                    var updatepassword = from x in _authdatabase.Users
                                         where user.Username == newchangePassword.username
                                         select x;

                    foreach (var pwd in _authdatabase.Users.Where(w => w.Username == newchangePassword.username))
                    {
                        pwd.Password = PasswordEncrypt.PasswordEncryption(newchangePassword.newpassword);
                        pwd.Changepassworddate = DateTime.Now;

                        
                    }
                    await _authdatabase.SaveChangesAsync();
                }
                else
                {
                    return BadRequest(new { message = "Confirm password is Wrong" });
                }
            }
            else
            {
                return BadRequest(new { message = "Old Password is Wrong" });
            }
            return Ok(new { message = "Change Password Successfully" });
        }

        [HttpPost("SendSMS")]
        public string Sms()
        {
            // Add below code in your application
            string url = "https://api2.juvlon.com/v4/sendTransactionalSMS";
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            myHttpWebRequest.Method = "POST";

            string postData = "{\"apiKey\":\"OTM2MTMjIyMyMDIzLTAxLTA2IDEyOjM5OjQy\"" +
                               ",\"mailerID\":\"28\"" +
                               ",\"subID\":\"98\"" +
                               ",\"mobile\":\"8825713156\"" +
                               ",\"email\":\"example@abc.com\"" +
                               ",\"listName\":\"ListABC\"" +
                                ",\"prefix\":\"Mr.\"" +
                                ",\"firstName\":\"John\"" +
                                ",\"middleName\":\"K\"" +
                                ",\"lastName\":\"Smith\"" +
                                ",\"phone\":\"+91227492682648\"" +
                                ",\"address\":\"House 5, Suncity\"" +
                                ",\"city\":\"Mumbai\"" +
                                ",\"state\":\"Maharashtra\"" +
                                ",\"pinCode\":\"9604\"" +
                                ",\"Country\":\"India\"" +
                                ",\"residencePhone\":\"912274926829\"" +
                                ",\"designation\":\"Project Manager\"" +
                                ",\"company\":\"XYZ ltd.\"" +
                                ",\"companyAddress\":\"Office no 5, ABC IT Park\"" +
                                ",\"companyCity\":\"Mumbai\"" +
                                ",\"companyState\":\"Maharashtra\"" +
                                ",\"companyCountry\":\"India\"" +
                                ",\"companyPin\":\"9607\"" +
                                ",\"companyPhone\":\"+912274926826\"" +
                                ",\"companyFax\":\"+912274926827\"" +
                                ",\"birthday\":\"1988-06-10\"" +
                                ",\"anniversary\":\"2015-06-11\"" +
                                ",\"referenceNo\":\"123\"" +
                                ",\"extra1\":\"98732\"" +
                                ",\"extra2\":\"989\"" +
                                ",\"extra3\":\"7687\"" +
                                ",\"extra4\":\"989\"" +
                                ",\"extra5\":\"7687\"" +
                                ",\"extra6\":\"K987JF\"" +
                                ",\"customField\":\"This is an Reminder for Tommorrow you have an appoinment with your doctor\"" + " }";

            byte[] data = Encoding.ASCII.GetBytes(postData);
            myHttpWebRequest.ContentType = "application/json";
            System.Net.ServicePointManager.SecurityProtocol |=
            SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ///ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            myHttpWebRequest.ContentLength = data.Length;
            Stream requestStream = myHttpWebRequest.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
            Stream responseStream = myHttpWebResponse.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);
            string pageContent = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            responseStream.Close();
            myHttpWebResponse.Close();
              

            return pageContent;
        }

            



    }
}