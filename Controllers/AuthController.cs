using Microsoft.AspNetCore.Mvc;
using SmartDoor.Models;
using SmartDoor.Services;
using System.Security.Cryptography;
using System.Text;
using ZstdSharp.Unsafe;

namespace SmartDoor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private static Dictionary<string, string> _otpMap = new Dictionary<string, string>();
        private User? user;
        private readonly IAuthService _authService;
        private readonly MQTTService? _mqttService;
        private readonly UserService? _userService;

        public AuthController(IAuthService authService, MQTTService mqttService, UserService userService)
        {
            _authService = authService;
            _mqttService = mqttService;
            _userService = userService;
        }

        [HttpPost]
        [Route("get-otp")]
        public async Task<IActionResult> GetOTP(LoginModel userInfo)
        {
            try
            {
                var token = await _authService.Authenticate(userInfo);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
            user = await _userService.GetAsync(userInfo.UserName);
            if(user == null)
            {
                var res = new ObjectResult("User not found");
                res.StatusCode = 333;
                return res;
            }    
            Random random = new Random();
            int randomNumber = random.Next(10000);
            string otp = randomNumber.ToString("D4");
            string payload = $"{otp}{user.Phone.TrimStart('0')}";

            await _mqttService.PublishAsync("ESP32/SEND_OTP", payload);

            var key = user.Phone + DateTime.Now.ToString();

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(key);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                key = Convert.ToBase64String(hashBytes);
            }

            _otpMap.Add(key, otp);

            return Ok(new { Key = key });
        }

        [HttpPost]
        [Route("login-otp")]
        public async Task<IActionResult> LoginWithOtp(LoginModel userInfo)
        {
            var otp = _otpMap[userInfo.Key];
            if(userInfo.Otp == otp)
            {
                Token token;
                try
                {
                    token = await _authService.Authenticate(userInfo);
                }
                catch (Exception ex)
                {
                    return Unauthorized(ex.Message);
                }
                return Ok(token);
            }
            var res = new ObjectResult("Wrong otp!");
            res.StatusCode = 333;
            return res;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginModel userInfo)
        {
            Token token;
            try
            {
                token = await _authService.Authenticate(userInfo);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
            return Ok(token);
        }
    }
}
