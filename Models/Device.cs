using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SmartDoor.Models
{
    public class DeviceRequest
    {
        public string? DeviceId { get; set; }
    }
    public class Device
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? Name { get; set; }
        public string? Type { get; set; }

    }
    public class Lock : Device
    {
        public static bool Status { get; set; }
    }
}
