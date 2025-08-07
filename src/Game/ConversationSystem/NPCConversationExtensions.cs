using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Extension methods for NPCs in the conversation system.
/// These methods provide conversation-specific functionality without modifying the core NPC class.
/// </summary>
public static class NPCConversationExtensions
{
    /// <summary>
    /// Check if the NPC has a letter to send.
    /// </summary>
    public static bool HasLetterToSend(this NPC npc)
    {
        // TODO: Check actual letter generation conditions
        // For now, randomly decide based on NPC properties
        return new Random(npc.ID.GetHashCode()).Next(100) < 30; // 30% chance
    }
    
    /// <summary>
    /// Generate a letter from this NPC.
    /// </summary>
    public static Letter GenerateLetter(this NPC npc)
    {
        var random = new Random();
        var tokenTypes = Enum.GetValues<ConnectionType>();
        var stakeTypes = Enum.GetValues<StakeType>();
        
        return new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = npc.ID,
            SenderName = npc.Name,
            RecipientId = "player", // TODO: Generate actual recipient
            RecipientName = "Someone Important",
            TokenType = tokenTypes[random.Next(tokenTypes.Length)],
            Stakes = stakeTypes[random.Next(stakeTypes.Length)],
            Size = (SizeCategory)random.Next(1, 4), // Small/Medium/Large
            DeadlineInHours = random.Next(3, 8), // 3-7 days
            State = LetterState.Offered,
            QueuePosition = -1 // Not in queue yet
        };
    }
    
    /// <summary>
    /// Check if the NPC knows a specific route.
    /// </summary>
    public static bool KnowsRoute(this NPC npc, RouteOption route)
    {
        // NPCs know routes based on their location and role
        // TODO: Implement actual route knowledge system
        return false;
    }
    
    /// <summary>
    /// Get an NPC contact that this NPC can introduce.
    /// </summary>
    public static NPC GetContact(this NPC npc)
    {
        // TODO: Implement actual contact system
        // For now, return a placeholder NPC
        return new NPC
        {
            ID = Guid.NewGuid().ToString(),
            Name = $"{npc.Name}'s Contact",
            Role = "Merchant",
            Location = npc.Location
        };
    }
    
    /// <summary>
    /// Get a secret route that this NPC knows.
    /// </summary>
    public static RouteOption GetSecretRoute(this NPC npc)
    {
        // TODO: Implement actual secret route system
        return new RouteOption
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"Secret path known to {npc.Name}",
            Origin = npc.Location,
            Destination = "SecretDestination",
            TravelTimeHours = 1,
            Method = TravelMethods.Walking
        };
    }
    
    /// <summary>
    /// Get a secret that this NPC holds.
    /// </summary>
    public static Information GetSecret(this NPC npc)
    {
        // TODO: Implement actual secret system
        return new Information
        {
            Id = Guid.NewGuid().ToString(),
            Description = $"Secret information from {npc.Name}",
        };
    }
    
    /// <summary>
    /// Add a known route to the NPC.
    /// </summary>
    public static void AddKnownRoute(this NPC npc, RouteOption route)
    {
        // TODO: Implement NPC route knowledge storage
        // For now, this is a no-op
    }
    
    /// <summary>
    /// Get the NPC's known routes.
    /// </summary>
    public static List<RouteOption> KnownRoutes(this NPC npc)
    {
        // TODO: Implement actual route knowledge retrieval
        return new List<RouteOption>();
    }
}

/// <summary>
/// Extension methods for Players in the conversation system.
/// </summary>
public static class PlayerConversationExtensions
{
    /// <summary>
    /// Check if the player has an obligation to a specific NPC.
    /// </summary>
    public static bool HasObligationTo(this Player player, NPC npc)
    {
        // TODO: Check actual obligations
        return false;
    }
    
    /// <summary>
    /// Add an obligation for the player.
    /// </summary>
    public static void AddObligation(this Player player, StandingObligation obligation)
    {
        // TODO: Implement obligation storage
        // For now, this is a no-op
    }
    
    /// <summary>
    /// Add a known NPC to the player's network.
    /// </summary>
    public static void AddKnownNPC(this Player player, string npcId)
    {
        // TODO: Implement NPC network storage
        // For now, this is a no-op
    }
    
    /// <summary>
    /// Add a known location to the player's map.
    /// </summary>
    public static void AddKnownLocation(this Player player, string locationId)
    {
        // TODO: Implement location discovery storage
        // For now, this is a no-op
    }
}

/// <summary>
/// Extension methods for GameWorld in the conversation system.
/// </summary>
public static class GameWorldConversationExtensions
{
    /// <summary>
    /// Add an NPC to the game world dynamically.
    /// </summary>
    public static void AddNPC(this GameWorld gameWorld, NPC npc)
    {
        // TODO: Implement dynamic NPC addition
        // For now, this is a no-op
    }
}