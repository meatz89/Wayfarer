using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Wayfarer.Core.Repositories;

namespace Wayfarer.Services
{
    /// <summary>
    /// Service for NPC-related business logic
    /// </summary>
    public class NPCService
    {
        private readonly INPCRepository _repository;
        private readonly ITimeManager _timeManager;
        private readonly ILogger<NPCService> _logger;

        public NPCService(INPCRepository repository, ITimeManager timeManager, ILogger<NPCService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get time block service planning data for UI display
        /// </summary>
        public List<TimeBlockServiceInfo> GetTimeBlockServicePlan(string locationId)
        {
            var timeBlockPlan = new List<TimeBlockServiceInfo>();
            var allTimeBlocks = Enum.GetValues<TimeBlocks>();
            var locationNPCs = _repository.GetNPCsForLocation(locationId).ToList();

            foreach (var timeBlock in allTimeBlocks)
            {
                var availableNPCs = locationNPCs.Where(npc => npc.IsAvailable(timeBlock)).ToList();
                var availableServices = availableNPCs.SelectMany(npc => npc.ProvidedServices).Distinct().ToList();

                timeBlockPlan.Add(new TimeBlockServiceInfo
                {
                    TimeBlock = timeBlock,
                    AvailableNPCs = availableNPCs,
                    AvailableServices = availableServices,
                    IsCurrentTimeBlock = timeBlock == _timeManager.GetCurrentTimeBlock()
                });
            }

            return timeBlockPlan;
        }

        /// <summary>
        /// Get all unique services available at a location across all time blocks
        /// </summary>
        public List<ServiceTypes> GetAllLocationServices(string locationId)
        {
            var locationNPCs = _repository.GetNPCsForLocation(locationId);
            return locationNPCs.SelectMany(npc => npc.ProvidedServices).Distinct().ToList();
        }

        /// <summary>
        /// Get service availability summary for a specific service across all time blocks
        /// </summary>
        public ServiceAvailabilityPlan GetServiceAvailabilityPlan(string locationId, ServiceTypes service)
        {
            var allTimeBlocks = Enum.GetValues<TimeBlocks>();
            var locationNPCs = _repository.GetNPCsForLocation(locationId).ToList();
            var serviceProviders = locationNPCs.Where(npc => npc.ProvidedServices.Contains(service)).ToList();

            var availableTimeBlocks = new List<TimeBlocks>();
            foreach (var timeBlock in allTimeBlocks)
            {
                if (serviceProviders.Any(npc => npc.IsAvailable(timeBlock)))
                {
                    availableTimeBlocks.Add(timeBlock);
                }
            }

            return new ServiceAvailabilityPlan
            {
                Service = service,
                AvailableTimeBlocks = availableTimeBlocks,
                ServiceProviders = serviceProviders
            };
        }

        /// <summary>
        /// Find NPCs that can provide a specific service at the current time
        /// </summary>
        public IEnumerable<NPC> GetAvailableServiceProviders(ServiceTypes service, string locationId = null)
        {
            var currentTime = _timeManager.GetCurrentTimeBlock();
            var npcs = string.IsNullOrWhiteSpace(locationId) 
                ? _repository.GetAvailableNPCs(currentTime)
                : _repository.GetNPCsForLocationAndTime(locationId, currentTime);

            return npcs.Where(n => n.CanProvideService(service));
        }

        /// <summary>
        /// Check if a specific NPC is available to interact with at the current time
        /// </summary>
        public bool IsNPCAvailable(string npcId)
        {
            var npc = _repository.GetById(npcId);
            if (npc == null)
            {
                _logger.LogWarning($"NPC with ID '{npcId}' not found");
                return false;
            }

            var currentTime = _timeManager.GetCurrentTimeBlock();
            return npc.IsAvailable(currentTime);
        }

        /// <summary>
        /// Get NPCs available for interaction at a specific location spot
        /// </summary>
        public IEnumerable<NPCInteractionInfo> GetAvailableInteractions(string locationSpotId)
        {
            var currentTime = _timeManager.GetCurrentTimeBlock();
            var npcs = _repository.GetNPCsForLocationSpotAndTime(locationSpotId, currentTime);

            return npcs.Select(npc => new NPCInteractionInfo
            {
                NPC = npc,
                IsAvailable = npc.IsAvailable(currentTime),
                AvailableServices = npc.IsAvailable(currentTime) ? npc.ProvidedServices : new List<ServiceTypes>(),
                UnavailableReason = !npc.IsAvailable(currentTime) ? "NPC is not available at this time" : null
            });
        }
    }

    /// <summary>
    /// Information about an NPC interaction opportunity
    /// </summary>
    public class NPCInteractionInfo
    {
        public NPC NPC { get; set; }
        public bool IsAvailable { get; set; }
        public List<ServiceTypes> AvailableServices { get; set; }
        public string UnavailableReason { get; set; }
    }
}