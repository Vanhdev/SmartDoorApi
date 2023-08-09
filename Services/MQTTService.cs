using Microsoft.AspNetCore.Mvc;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Internal;
using MQTTnet.Protocol;
using System.Text;

namespace SmartDoor.Services
{
    //public interface IMQTTService
    //{
    //    Task<MqttClientPublishResult> PublishAsync(string topic, string payload);
    //}

    public class MQTTService
    {
        //static DeviceService? _deviceService;
        static IMqttClient? mqttClient;

        //public MQTTService(DeviceService deviceService) 
        //{
        //    _deviceService = deviceService;
        //}

        public static IMqttClient Start(string host, int port)
        {
            string cid = Guid.NewGuid().ToString();
            mqttClient = new MqttFactory().CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                              .WithTcpServer(host, port)
                              .WithClientId(cid)
                              .WithCleanSession()
                              .Build();

            mqttClient.ConnectAsync(options).Wait();

            mqttClient.SubscribeAsync("ESP32/LOCK_RESPONSE");
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
                return Task.CompletedTask;
            };

            mqttClient.SubscribeAsync("Create device");
            //mqttClient.ApplicationMessageReceivedAsync += async e =>
            //{
            //    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            //    string[] infos = payload.Split(',');

            //    await _deviceService.CreateAsync(new Models.Device { 
            //                                        Id = new Guid(),
            //                                        Name = infos[0],
            //                                        Type = infos[1]
            //                                    }) ;
            //};
            return mqttClient;
        }

        public async Task<MqttClientPublishResult> PublishAsync(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                    .Build();
            var result = await mqttClient.PublishAsync(message);

            return result;
        }
    }
}
