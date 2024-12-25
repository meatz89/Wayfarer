public class GameSetup
{
    public static GameState CreateNewGame()
    {
        GameState gameState = new GameState();

        gameState.Locations = new List<Location>
        {
            new Location() { Index = 1, Name = "Docks", Description = "Docks" },
            new Location() { Index = 2, Name = "Market", Description = "Market" }
        };
        gameState.CurrentLocation = "Docks";

        var playerInfo = new PlayerInfo();
        playerInfo.Money = 10;

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

        return gameState;
    }
}
