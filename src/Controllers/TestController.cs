using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly GameWorld _gameWorld;
        
        public TestController(GameWorld gameWorld)
        {
            _gameWorld = gameWorld;
        }
        
        [HttpGet("startup")]
        public IActionResult TestStartup()
        {
            try
            {
                // Check if GameWorld was initialized
                if (_gameWorld == null)
                {
                    return Ok(new
                    {
                        success = false,
                        errors = new[] { "GameWorld is null - initialization failed completely" }
                    });
                }
                
                // Check if WorldState exists
                if (_gameWorld.WorldState == null)
                {
                    return Ok(new
                    {
                        success = false,
                        errors = new[] { "WorldState is null - initialization incomplete" }
                    });
                }
                
                // Check for locations
                int locationCount = _gameWorld.WorldState.locations?.Count ?? 0;
                int spotCount = _gameWorld.WorldState.locationSpots?.Count ?? 0;
                int npcCount = _gameWorld.WorldState.NPCs?.Count ?? 0;
                
                // Check for player
                bool hasPlayer = _gameWorld.GetPlayer() != null;
                
                // Check for letter queue
                int letterCount = 0;
                if (hasPlayer && _gameWorld.GetPlayer().LetterQueue != null)
                {
                    letterCount = _gameWorld.GetPlayer().LetterQueue.Count(l => l != null);
                }
                
                // Determine if startup was successful
                bool success = locationCount > 0 && hasPlayer;
                
                var response = new
                {
                    success = success,
                    stats = new
                    {
                        locations = locationCount,
                        spots = spotCount,
                        npcs = npcCount,
                        hasPlayer = hasPlayer,
                        lettersInQueue = letterCount
                    }
                };
                
                if (!success)
                {
                    var errors = new List<string>();
                    if (locationCount == 0) errors.Add("No locations loaded");
                    if (!hasPlayer) errors.Add("Player not initialized");
                    
                    return Ok(new
                    {
                        success = false,
                        stats = response.stats,
                        errors = errors
                    });
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    errors = new[] { $"Exception during test: {ex.Message}" },
                    stackTrace = ex.StackTrace
                });
            }
        }
        
        [HttpGet("phase-errors")]
        public IActionResult GetPhaseErrors()
        {
            // This endpoint could be extended to capture and return phase-specific errors
            // For now, just return basic info
            return Ok(new
            {
                message = "Check console output for phase errors",
                hint = "Phase 2 errors are typically validation issues with NPCs or location spots"
            });
        }
    }
}