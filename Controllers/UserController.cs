using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using SmartDoor.Models;
using SmartDoor.Services;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace SmartDoor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService) =>
            _userService = userService;

        [HttpGet]
        public async Task<List<User>> Get() =>
            await _userService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<User>> Get(string name)
        {
            var book = await _userService.GetAsync(name);

            if (book is null)
            {
                return NotFound();
            }

            return book;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] User newUser)
        {
            if(!newUser.Phone.All(char.IsNumber))
            {
                throw new Exception("Số điện thoại không hợp lệ!");
            }
            string hashedPassword;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(newUser.Password);
                byte[] hash = sha256.ComputeHash(bytes);
                hashedPassword = Convert.ToBase64String(hash);
            }

            newUser.Password = hashedPassword;

            await _userService.CreateAsync(newUser);

            return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
        }

        
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, User updatedUser)
        {
            var user = await _userService.GetAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            updatedUser.Id = user.Id;

            MergeObjects<User>(updatedUser, user);

            await _userService.UpdateAsync(id, updatedUser);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var book = await _userService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            await _userService.RemoveAsync(id);

            return NoContent();
        }

        [Authorize]
        [HttpGet]
        [Route("get-devices")]
        public async Task<IActionResult> GetDevices(string name)
        {
            var lst = await _userService.GetDevicesAsync(name);
            return Ok( lst);
        }

        [Authorize]
        [HttpGet]
        [Route("get-device")]
        public async Task<IActionResult> GetDevice(string name, string deviceName)
        {
            var lst = await _userService.GetDevicesAsync(name);
            var device = lst.FirstOrDefault(x => x.Name == deviceName);
            return Ok(device);
        }

        public static void MergeObjects<T>(T target, T source)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if(property.Name != "Devices")
                {
                    object? sourceValue = property.GetValue(source, null);
                    if (sourceValue != null)
                    {
                        property.SetValue(target, sourceValue, null);
                    }
                }
                
            }
        }
    }
}
