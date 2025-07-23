using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wayfarer.Core.Repositories;
using Wayfarer.Core.Repositories.Implementation;
using Wayfarer.Services;

namespace Wayfarer.Core.Repositories
{
    /// <summary>
    /// Helper class to assist with migration from old repositories to new pattern
    /// </summary>
    public static class RepositoryMigrationHelper
    {
        /// <summary>
        /// Register repository implementations and related services
        /// </summary>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Register the world state accessor
            services.AddScoped<IWorldStateAccessor, GameWorldStateAccessor>();

            // Register repository interfaces with their implementations
            services.AddScoped<IItemRepository, ItemRepositoryImpl>();
            services.AddScoped<INPCRepository, NPCRepositoryImpl>();
            services.AddScoped<ILetterTemplateRepository, LetterTemplateRepositoryImpl>();
            services.AddScoped<ILocationRepository, LocationRepositoryImpl>();
            services.AddScoped<ILocationSpotRepository, LocationSpotRepositoryImpl>();
            services.AddScoped<IRouteRepository, RouteRepositoryImpl>();

            // Register services that contain extracted business logic
            services.AddScoped<NPCService>();
            services.AddScoped<LetterGenerationService>();

            return services;
        }

        /// <summary>
        /// Create a facade that maintains backward compatibility while using new repositories
        /// </summary>
        public static ItemRepository CreateItemRepositoryFacade(IServiceProvider serviceProvider)
        {
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var newRepo = serviceProvider.GetRequiredService<IItemRepository>();
            
            // Return a wrapper that delegates to the new repository
            return new ItemRepositoryFacade(gameWorld, newRepo);
        }

        /// <summary>
        /// Create a facade that maintains backward compatibility while using new repositories
        /// </summary>
        public static NPCRepository CreateNPCRepositoryFacade(IServiceProvider serviceProvider)
        {
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var debugLogger = serviceProvider.GetRequiredService<DebugLogger>();
            var newRepo = serviceProvider.GetRequiredService<INPCRepository>();
            var npcService = serviceProvider.GetRequiredService<NPCService>();
            
            // Return a wrapper that delegates to the new repository and service
            return new NPCRepositoryFacade(gameWorld, debugLogger, newRepo, npcService);
        }

        /// <summary>
        /// Create a facade that maintains backward compatibility while using new repositories
        /// </summary>
        public static LetterTemplateRepository CreateLetterTemplateRepositoryFacade(IServiceProvider serviceProvider)
        {
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var newRepo = serviceProvider.GetRequiredService<ILetterTemplateRepository>();
            var letterService = serviceProvider.GetRequiredService<LetterGenerationService>();
            
            // Return a wrapper that delegates to the new repository and service
            return new LetterTemplateRepositoryFacade(gameWorld, newRepo, letterService);
        }
    }

    /// <summary>
    /// Temporary facade to maintain backward compatibility for ItemRepository
    /// </summary>
    internal class ItemRepositoryFacade : ItemRepository
    {
        private readonly IItemRepository _newRepository;

        public ItemRepositoryFacade(GameWorld gameWorld, IItemRepository newRepository) 
            : base(gameWorld)
        {
            _newRepository = newRepository;
        }

        public override Item GetItemById(string id) => _newRepository.GetById(id);
        public override Item GetItemByName(string name) => _newRepository.GetByName(name);
        public override List<Item> GetAllItems() => _newRepository.GetAll().ToList();
        public override List<Item> GetItemsForLocation(string locationId, string spotId = null) 
            => _newRepository.GetItemsForLocation(locationId, spotId).ToList();
        public override void AddItem(Item item) => _newRepository.Add(item);
        public override void UpdateItem(Item item) => _newRepository.Update(item);
        public override bool RemoveItem(string id) => _newRepository.Remove(id);
    }

    /// <summary>
    /// Temporary facade to maintain backward compatibility for NPCRepository
    /// </summary>
    internal class NPCRepositoryFacade : NPCRepository
    {
        private readonly INPCRepository _newRepository;
        private readonly NPCService _npcService;

        public NPCRepositoryFacade(GameWorld gameWorld, DebugLogger debugLogger, 
            INPCRepository newRepository, NPCService npcService) 
            : base(gameWorld, debugLogger)
        {
            _newRepository = newRepository;
            _npcService = npcService;
        }

        public override NPC GetNPCById(string id) => _newRepository.GetById(id);
        public override List<NPC> GetAllNPCs() => _newRepository.GetAll().ToList();
        public override List<NPC> GetNPCsForLocation(string locationId) 
            => _newRepository.GetNPCsForLocation(locationId).ToList();
        public override List<NPC> GetAvailableNPCs(TimeBlocks currentTime) 
            => _newRepository.GetAvailableNPCs(currentTime).ToList();
        public override List<TimeBlockServiceInfo> GetTimeBlockServicePlan(string locationId) 
            => _npcService.GetTimeBlockServicePlan(locationId);
        public override void AddNPC(NPC npc) => _newRepository.Add(npc);
        public override void UpdateNPC(NPC npc) => _newRepository.Update(npc);
        public override bool RemoveNPC(string id) => _newRepository.Remove(id);
    }

    /// <summary>
    /// Temporary facade to maintain backward compatibility for LetterTemplateRepository
    /// </summary>
    internal class LetterTemplateRepositoryFacade : LetterTemplateRepository
    {
        private readonly ILetterTemplateRepository _newRepository;
        private readonly LetterGenerationService _letterService;

        public LetterTemplateRepositoryFacade(GameWorld gameWorld, 
            ILetterTemplateRepository newRepository, LetterGenerationService letterService) 
            : base(gameWorld)
        {
            _newRepository = newRepository;
            _letterService = letterService;
        }

        public override List<LetterTemplate> GetAllTemplates() => _newRepository.GetAll().ToList();
        public override LetterTemplate GetTemplateById(string templateId) => _newRepository.GetById(templateId);
        public override Letter GenerateLetterFromTemplate(LetterTemplate template, string senderName, string recipientName)
            => _letterService.GenerateLetterFromTemplate(template, senderName, recipientName);
        public override Letter GenerateForcedLetterFromTemplate(LetterTemplate template)
            => _letterService.GenerateForcedLetterFromTemplate(template);
    }
}