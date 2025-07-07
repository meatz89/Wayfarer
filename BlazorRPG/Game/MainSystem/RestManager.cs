public class RestManager
{
    public List<RestOption> GetAvailableRestOptions(GameWorld gameWorld)
    {
        Location currentLocation = gameWorld.Locations.Find(l => l.Id == gameWorld.CurrentLocation.Id);
        if (currentLocation == null || currentLocation.RestOptions == null)
        {
            return new List<RestOption>();
        }

        List<RestOption> availableOptions = new List<RestOption>();
        foreach (RestOption option in currentLocation.RestOptions)
        {
            if (option.IsAvailable)
            {
                if (option.RequiredItem == null ||
                    gameWorld.PlayerInventory.ItemSlots.Contains(option.RequiredItem))
                {
                    availableOptions.Add(option);
                }
            }
        }

        return availableOptions;
    }

    public void Rest(GameWorld gameWorld, RestOption option)
    {
        // Deduct cost
        gameWorld.PlayerCoins -= option.CoinCost;

        // Recover stamina
        gameWorld.PlayerStamina += option.StaminaRecovery;
        Player player = gameWorld.GetPlayer();
        if (gameWorld.PlayerStamina > player.MaxStamina)
        {
            gameWorld.PlayerStamina = player.MaxStamina;
        }

        if (option.EnablesDawnDeparture)
        {
            // Set next time block to Dawn
            gameWorld.CurrentTimeBlock = TimeBlocks.Dawn;
        }
        else
        {
            // Standard rest advances to next day Morning
            gameWorld.CurrentDay++;
            gameWorld.CurrentTimeBlock = TimeBlocks.Morning;
        }
    }
}