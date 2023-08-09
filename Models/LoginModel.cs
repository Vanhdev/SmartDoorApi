namespace SmartDoor.Models
{
    public class LoginModel
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string? Otp { get; set; }
        public string? Key { get; set;}
    }
}
