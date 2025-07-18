using System.Collections.Generic;

    /// <summary>
    /// Represents how a route can be discovered through NPC relationships
    /// </summary>
    public class RouteDiscovery
    {
        /// <summary>
        /// The route ID this discovery unlocks
        /// </summary>
        public string RouteId { get; set; } = "";
        
        /// <summary>
        /// NPCs who know about this route and can teach it
        /// </summary>
        public List<string> KnownByNPCs { get; set; } = new List<string>();
        
        /// <summary>
        /// How many tokens (total across all types) the player needs with an NPC to learn this route
        /// </summary>
        public int RequiredTokensWithNPC { get; set; } = 3;
        
        /// <summary>
        /// Context-specific discovery information for each NPC who knows this route
        /// </summary>
        public Dictionary<string, RouteDiscoveryContext> DiscoveryContexts { get; set; } = new Dictionary<string, RouteDiscoveryContext>();
    }
    
    /// <summary>
    /// Context-specific requirements and narrative for discovering a route from a specific NPC
    /// </summary>
    public class RouteDiscoveryContext
    {
        /// <summary>
        /// Equipment that must be owned to convince this specific NPC to share the route
        /// </summary>
        public List<string> RequiredEquipment { get; set; } = new List<string>();
        
        /// <summary>
        /// Narrative shown when discovering this route from this NPC
        /// </summary>
        public string Narrative { get; set; } = "";
    }
