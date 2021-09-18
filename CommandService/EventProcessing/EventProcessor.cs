using System;
using System.Text.Json;
using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CommandService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
        }

        public void ProcessEvent(string message)
        {
            EventType eventType = DetermineEvent(message);
            switch (eventType)
            {
                case EventType.PlatformPublished:
                    AddPlatform(message);
                    break;               
                default:
                    break;
            }
        }

        private EventType DetermineEvent(string notificationMessage)
        {
            Console.WriteLine("--> Attempt to determine the event type...");

            var message = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

            switch (message.Event)
            {
                case "Platform:Published":
                    Console.WriteLine("--> Platform:Published event detected.");
                    return EventType.PlatformPublished;
                default:
                    Console.WriteLine("--> Could not determine event type.");
                    return EventType.Unknown;
            }
        }

        private void AddPlatform(string platformPublishedMessage)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                 var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();
                 var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);

                 try
                 {
                      var plat = _mapper.Map<Platform>(platformPublishedDto);

                      if(!repo.ExternalPlatformExists(plat.ExternalId))
                      {
                          repo.CreatePlatform(plat);
                          repo.SaveChanges();
                      }
                      else
                      {
                          Console.WriteLine($"--> Platform with ID {plat.ExternalId} already exists");
                      }
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine($"Could not add Platform to database: {ex.Message}");
                 }
            }
        }
    }
}