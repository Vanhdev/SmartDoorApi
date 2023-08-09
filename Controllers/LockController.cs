using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartDoor.Models;
using SmartDoor.Services;

namespace SmartDoor.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LockController : ControllerBase
    {
        private readonly MQTTService? _mqttService;
        private readonly DeviceService? _deviceService;
        
        public LockController(MQTTService mqttService) => _mqttService = mqttService;

        [HttpPost]
        public async Task<IActionResult> SetLock(Device deviceInfo)
        {
            var device = await _deviceService.GetAsync(deviceInfo.Name);

            if (device == null)
            {
                var res = new ObjectResult("Device not found");
                res.StatusCode = 333;
                return res;
            }

            return Ok(device);
        }
    }
}
