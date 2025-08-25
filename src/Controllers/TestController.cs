using Microsoft.AspNetCore.Mvc;
using System;

namespace Wayfarer.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly GameWorld _gameWorld;
        private readonly GameFacade _gameFacade;
        
        public TestController(GameWorld gameWorld, GameFacade gameFacade)
        {
            Console.WriteLine("[TestController] Constructor called");
            _gameWorld = gameWorld;
            _gameFacade = gameFacade;
            Console.WriteLine("[TestController] Constructor completed");
        }
        
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            Console.WriteLine("[TestController] Ping endpoint hit");
            return Ok(new { message = "pong", time = DateTime.UtcNow });
        }
        
        [HttpGet("gameworld")]
        public IActionResult TestGameWorld()
        {
            Console.WriteLine("[TestController] GameWorld test endpoint hit");
            var player = _gameWorld.GetPlayer();
            return Ok(new 
            { 
                playerName = player?.Name ?? "No player",
                isInitialized = player?.IsInitialized ?? false
            });
        }
    }
}