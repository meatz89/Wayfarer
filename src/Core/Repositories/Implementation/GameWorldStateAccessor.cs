using System.Collections.Generic;

namespace Wayfarer.Core.Repositories.Implementation
{
    /// <summary>
    /// Adapter that provides IWorldStateAccessor interface over GameWorld
    /// </summary>
    public class GameWorldStateAccessor : IWorldStateAccessor
    {
        private readonly GameWorld _gameWorld;

        public GameWorldStateAccessor(GameWorld gameWorld)
        {
            _gameWorld = gameWorld ?? throw new System.ArgumentNullException(nameof(gameWorld));
        }

        public List<Item> Items
        {
            get
            {
                if (_gameWorld.WorldState.Items == null)
                {
                    _gameWorld.WorldState.Items = new List<Item>();
                }
                return _gameWorld.WorldState.Items;
            }
        }

        public List<NPC> NPCs
        {
            get
            {
                var npcs = _gameWorld.WorldState.GetCharacters();
                if (npcs == null)
                {
                    // Initialize if needed - this maintains existing behavior
                    return new List<NPC>();
                }
                return npcs;
            }
        }

        public List<LetterTemplate> LetterTemplates
        {
            get
            {
                if (_gameWorld.WorldState.LetterTemplates == null)
                {
                    _gameWorld.WorldState.LetterTemplates = new List<LetterTemplate>();
                }
                return _gameWorld.WorldState.LetterTemplates;
            }
        }

        public List<Location> Locations
        {
            get
            {
                if (_gameWorld.WorldState.Locations == null)
                {
                    _gameWorld.WorldState.Locations = new List<Location>();
                }
                return _gameWorld.WorldState.Locations;
            }
        }

        public List<LocationSpot> LocationSpots
        {
            get
            {
                if (_gameWorld.WorldState.LocationSpots == null)
                {
                    _gameWorld.WorldState.LocationSpots = new List<LocationSpot>();
                }
                return _gameWorld.WorldState.LocationSpots;
            }
        }

        public List<RouteOption> Routes
        {
            get
            {
                if (_gameWorld.WorldState.Routes == null)
                {
                    _gameWorld.WorldState.Routes = new List<RouteOption>();
                }
                return _gameWorld.WorldState.Routes;
            }
        }

        public ITimeManager TimeManager => new TimeManagerAdapter(_gameWorld.TimeManager);
    }
}