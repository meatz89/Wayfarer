using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Wayfarer.Core.Repositories;
using Wayfarer.Services;

namespace Wayfarer.Core.Repositories.Tests
{
    [TestFixture]
    public class NPCServiceTests
    {
        private Mock<INPCRepository> _mockRepository;
        private Mock<ITimeManager> _mockTimeManager;
        private Mock<ILogger<NPCService>> _mockLogger;
        private NPCService _service;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<INPCRepository>();
            _mockTimeManager = new Mock<ITimeManager>();
            _mockLogger = new Mock<ILogger<NPCService>>();
            _service = new NPCService(_mockRepository.Object, _mockTimeManager.Object, _mockLogger.Object);
        }

        [Test]
        public void GetTimeBlockServicePlan_ReturnsCorrectPlanForLocation()
        {
            // Arrange
            var locationId = "market_square";
            var npcs = new List<NPC>
            {
                new NPC 
                { 
                    ID = "npc1", 
                    Name = "Baker", 
                    Location = locationId,
                    ProvidedServices = new List<ServiceTypes> { ServiceTypes.Food }
                },
                new NPC 
                { 
                    ID = "npc2", 
                    Name = "Blacksmith", 
                    Location = locationId,
                    ProvidedServices = new List<ServiceTypes> { ServiceTypes.Equipment }
                }
            };

            _mockRepository.Setup(r => r.GetNPCsForLocation(locationId))
                .Returns(npcs);
            _mockTimeManager.Setup(t => t.GetCurrentTimeBlock())
                .Returns(TimeBlocks.Morning);

            // Act
            var result = _service.GetTimeBlockServicePlan(locationId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(Enum.GetValues<TimeBlocks>().Length));
            
            var morningPlan = result.First(p => p.TimeBlock == TimeBlocks.Morning);
            Assert.That(morningPlan.IsCurrentTimeBlock, Is.True);
            Assert.That(morningPlan.AvailableNPCs.Count, Is.EqualTo(2));
            Assert.That(morningPlan.AvailableServices, Contains.Item(ServiceTypes.Food));
            Assert.That(morningPlan.AvailableServices, Contains.Item(ServiceTypes.Equipment));
        }

        [Test]
        public void GetAvailableServiceProviders_FiltersCorrectly()
        {
            // Arrange
            var service = ServiceTypes.Food;
            var locationId = "market_square";
            var currentTime = TimeBlocks.Morning;

            var npcs = new List<NPC>
            {
                new NPC 
                { 
                    ID = "npc1", 
                    Name = "Baker",
                    Location = locationId,
                    ProvidedServices = new List<ServiceTypes> { ServiceTypes.Food }
                },
                new NPC 
                { 
                    ID = "npc2", 
                    Name = "Blacksmith",
                    Location = locationId,
                    ProvidedServices = new List<ServiceTypes> { ServiceTypes.Equipment }
                }
            };

            _mockTimeManager.Setup(t => t.GetCurrentTimeBlock()).Returns(currentTime);
            _mockRepository.Setup(r => r.GetNPCsForLocationAndTime(locationId, currentTime))
                .Returns(npcs);

            // Act
            var result = _service.GetAvailableServiceProviders(service, locationId).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Baker"));
        }

        [Test]
        public void IsNPCAvailable_ReturnsFalseForNonExistentNPC()
        {
            // Arrange
            var npcId = "non_existent";
            _mockRepository.Setup(r => r.GetById(npcId)).Returns((NPC)null);

            // Act
            var result = _service.IsNPCAvailable(npcId);

            // Assert
            Assert.That(result, Is.False);
            _mockLogger.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"NPC with ID '{npcId}' not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        }

        [Test]
        public void GetAvailableInteractions_HandlesUnavailableNPCs()
        {
            // Arrange
            var spotId = "tavern_bar";
            var currentTime = TimeBlocks.Night;
            
            var npc = new NPC 
            { 
                ID = "npc1", 
                Name = "Tavern Keeper",
                SpotId = spotId,
                ProvidedServices = new List<ServiceTypes> { ServiceTypes.Food, ServiceTypes.Drink }
            };

            // Mock NPC as unavailable at night
            Mock.Get(npc).Setup(n => n.IsAvailable(currentTime)).Returns(false);

            _mockTimeManager.Setup(t => t.GetCurrentTimeBlock()).Returns(currentTime);
            _mockRepository.Setup(r => r.GetNPCsForLocationSpotAndTime(spotId, currentTime))
                .Returns(new List<NPC> { npc });

            // Act
            var result = _service.GetAvailableInteractions(spotId).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            var interaction = result[0];
            Assert.That(interaction.IsAvailable, Is.False);
            Assert.That(interaction.AvailableServices, Is.Empty);
            Assert.That(interaction.UnavailableReason, Is.Not.Null);
        }
    }
}