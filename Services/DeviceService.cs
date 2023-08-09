using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SmartDoor.Models;

namespace SmartDoor.Services
{
    public class DeviceService
    {
        private readonly IMongoCollection<Device> _devicesCollection;

        public DeviceService(
            IOptions<EmbeddedSystemDatabaseSettings> embeddedSystemDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                embeddedSystemDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                embeddedSystemDatabaseSettings.Value.DatabaseName);

            _devicesCollection = mongoDatabase.GetCollection<Device>("devices");
        }

        public async Task<List<Device>> GetAllAsync() => 
            await _devicesCollection.Find(_ => true).ToListAsync();

        public async Task<Device> GetAsync(string name) =>
            await _devicesCollection.Find(x => x.Name == name).FirstOrDefaultAsync();

        public async Task CreateAsync(Device device) =>
            await _devicesCollection.InsertOneAsync(device);
    }
}
