public class GameSetup
{
    public static GameState CreateNewGame()
    {
        GameState gameState = new GameState();
        gameState.SetNewLocation(LocationNames.HarborStreets);
        gameState.SetNewTime(9);

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

        Inventory playerInventory = new Inventory();
        playerInventory.Food = 1;

        Player playerInfo = new Player();
        playerInfo.Coins = GameRules.StartingCoins;

        playerInfo.Health = GameRules.StartingHealth;
        playerInfo.MinHealth = GameRules.MinimumHealth;
        playerInfo.MaxHealth = 10;

        playerInfo.PhysicalEnergy = GameRules.StartingPhysicalEnergy;
        playerInfo.MaxPhysicalEnergy = 10;

        playerInfo.FocusEnergy = GameRules.StartingFocusEnergy;
        playerInfo.MaxFocusEnergy = 10;

        playerInfo.SocialEnergy = GameRules.StartingSocialEnergy;
        playerInfo.MaxSocialEnergy = 10;

        Dictionary<SkillTypes, int> skills = new Dictionary<SkillTypes, int>();
        skills[SkillTypes.Strength] = 1;
        skills[SkillTypes.Observance] = 1;
        skills[SkillTypes.Charisma] = 1;

        playerInfo.Skills = skills;
        playerInfo.Inventory = playerInventory;

        gameState.Player = playerInfo;


        return gameState;
    }
}
