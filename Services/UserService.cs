using SmartDoor.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace SmartDoor.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UserService(
            IOptions<EmbeddedSystemDatabaseSettings> embeddedSystemDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                embeddedSystemDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                embeddedSystemDatabaseSettings.Value.DatabaseName);

            _usersCollection = mongoDatabase.GetCollection<User>("users");
        }

        public async Task<List<User>> GetAsync() =>
            await _usersCollection.Find(_ => true).ToListAsync();

        public async Task<User> GetAsync(string name) =>
            await _usersCollection.Find(x => x.UserName == name).FirstOrDefaultAsync();

        public async Task CreateAsync(User newUser) =>
            await _usersCollection.InsertOneAsync(newUser);

        public async Task UpdateAsync(string id, User updatedUser) =>
            await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

        public async Task RemoveAsync(string id) =>
            await _usersCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<List<Device>> GetDevicesAsync(string name)
        {
            List<Device> lst = new();
            var User = await _usersCollection.Find(x => x.UserName == name).FirstOrDefaultAsync();
            if(User.Devices is not null)
            {
                lst.AddRange(User.Devices);
            }
            return lst;
        }
    }
}
