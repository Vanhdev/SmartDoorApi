using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SmartDoor.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        [BsonElement("Name")]
        public required string UserName { get; set; }
        
        public required string Password { get; set; }

        public required string Phone { get; set; }

        public required string Role { get; set; }

        public List<Device>? Devices { get; set; }

    }
    public class UserName
    {
        public string? Name { get; set; }
    }
}
