using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory() 
            {
                HostName = _configuration["RabbitMqHost"],
                Port = _configuration.GetValue<int>("RabbitMqPort")
            };

            try
            {
                 _connection = factory.CreateConnection();
                 _channel = _connection.CreateModel();

                 _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);

                 _connection.ConnectionShutdown += RabbitMq_ConnectionShutdown;

                 Console.WriteLine("--> Connection to message bus succeeded.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to the message bus: {ex.Message}");
                throw;
            }
        }

        public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);

            if(_connection.IsOpen)
            {
                Console.WriteLine("--> RabbitMq connection open, sending message...");
                SendMessage(message);
            }
            else
            {
                Console.WriteLine("--> RabbitMq connection closed, cannot send message.");
            }
        }

        public void Dispose()
        {
            if(_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "trigger", routingKey: "", basicProperties: null, body: body);
            Console.WriteLine($"Message published: {message}");
        }

        private void RabbitMq_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMq Connection Shut Down.");
        }
    }
}