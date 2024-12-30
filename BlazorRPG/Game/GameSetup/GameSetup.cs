public class GameSetup
{
    public static GameState CreateNewGame()
    {
        GameState gameState = new GameState();
        gameState.SetCurrentLocation(LocationNames.HarborStreets);

        gameState.Locations = new List<Location>
        {
            new Location() {
                Name = LocationNames.HarborStreets,
                Description = "Harbor Streets",
                ConnectedLocations = new() {
                    LocationNames.Docks,
                    LocationNames.MarketSquare,
                    LocationNames.LionsHeadTavern,
                    LocationNames.DarkForest,
                } },

            new Location() {
                Name = LocationNames.Docks,
                Description = "Docks",
                ConnectedLocations = new() {
                    LocationNames.HarborStreets,
                    LocationNames.MarketSquare
                } },

            new Location() {
                Name = LocationNames.MarketSquare,
                Description = "Market",
                ConnectedLocations = new() {
                    LocationNames.HarborStreets,
                    LocationNames.Docks
                } },

            new Location() {
                Name = LocationNames.LionsHeadTavern,
                Description = "Lion's Head Tavern",
                ConnectedLocations = new() {
                    LocationNames.HarborStreets,
                } },

            new Location() {
                Name = LocationNames.DarkForest,
                Description = "Dark Forest",
                ConnectedLocations = new() {
                    LocationNames.HarborStreets,
                } }
        };

        PlayerInventory playerInventory = new PlayerInventory();
        playerInventory.Food = 1;

        PlayerInfo playerInfo = new PlayerInfo();
        playerInfo.Coins = 5;

        playerInfo.Health = 8;
        playerInfo.MaxHealth = 10;

        playerInfo.PhysicalEnergy = 5;
        playerInfo.MaxPhysicalEnergy = 10;

        playerInfo.FocusEnergy = 5;
        playerInfo.MaxFocusEnergy = 10;

        playerInfo.SocialEnergy = 5;
        playerInfo.MaxSocialEnergy = 10;

        Dictionary<SkillTypes, int> skills = new Dictionary<SkillTypes, int>();
        skills[SkillTypes.Strength] = 1;
        skills[SkillTypes.Observance] = 1;
        skills[SkillTypes.Charisma] = 1;

        playerInfo.Skills = skills;

        gameState.PlayerInfo = playerInfo;
        gameState.PlayerInventory = playerInventory;

        return gameState;
    }
}
