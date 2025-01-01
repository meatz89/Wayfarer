public class LocationSpotBuilder
{
    private LocationNames locationName;
    private LocationSpotTypes spotName;
    private ResourceTypes? inputResource;
    private ResourceTypes? outputResource;
    private int inputAmount = 0;
    private int outputAmount = 0;
    private int coinCost = 0;
    private int coinReward = 0;
    private AccessTypes accessType;

    private int physicalEnergyCost;
    private int socialEnergyCost;
    private int focusEnergyCost;
    private int physicalEnergyReward;
    private int socialEnergyReward;
    private int focusEnergyReward;

    public LocationSpotBuilder ForLocation(LocationNames location)
    {
        this.locationName = location;
        return this;
    }

    public LocationSpotBuilder ForFeatureType(LocationSpotTypes featureType)
    {
        this.spotName = featureType;
        return this;
    }

    public LocationSpotBuilder WithInputResource(ResourceTypes resource, int amount)
    {
        inputResource = resource;
        inputAmount = amount;
        return this;
    }

    public LocationSpotBuilder WithOutputResource(ResourceTypes resource, int amount)
    {
        outputResource = resource;
        outputAmount = amount;
        return this;
    }

    public LocationSpotBuilder WithEnergyCost(int amount, EnergyTypes type)
    {
        switch(type)
        {
            case EnergyTypes.Physical:
                physicalEnergyCost = amount;
                break;

            case EnergyTypes.Focus:
                focusEnergyCost = amount;
                break;

            case EnergyTypes.Social:
                socialEnergyCost = amount;
                break;
        }
        return this;
    }

    public LocationSpotBuilder WithEnergyReward(int amount, EnergyTypes type)
    {
        switch (type)
        {
            case EnergyTypes.Physical:
                physicalEnergyReward = amount;
                break;

            case EnergyTypes.Focus:
                focusEnergyReward = amount;
                break;

            case EnergyTypes.Social:
                socialEnergyReward = amount;
                break;
        }
        return this;
    }

    internal LocationSpotBuilder WithCoinCost(int coinsToInvest)
    {
        coinCost = coinsToInvest;
        return this;
    }

    public LocationSpotBuilder WithCoinReward(int coinsToReceive)
    {
        coinReward = coinsToReceive;
        return this;
    }

    internal LocationSpotBuilder SetAccessType(AccessTypes accessType)
    {
        this.accessType = accessType;
        return this;
    }

    public LocationSpot Build()
    {
        // Each spot type determines what kind of action it supports
        BasicAction action = spotName switch
        {
            // Processing spots support labor actions that convert resources
            LocationSpotTypes.WoodworkBench or
            LocationSpotTypes.SmithyForge or
            LocationSpotTypes.TanningRack or
            LocationSpotTypes.WeavingLoom =>
                BuildProcessingAction(),

            // Trading spots support buying/selling with coins
            LocationSpotTypes.GeneralStore or
            LocationSpotTypes.ResourceMarket or
            LocationSpotTypes.SpecialtyShop or
            LocationSpotTypes.TavernBar =>
                BuildTradingAction(),

            // Gathering spots produce resources from environment
            LocationSpotTypes.ForestGrove or
            LocationSpotTypes.MineralDeposit or
            LocationSpotTypes.HuntingSpot =>
                BuildGatheringAction(),

            // Social spots support various interaction types
            LocationSpotTypes.CommonArea or
            LocationSpotTypes.ServingArea or
            LocationSpotTypes.PrivateCorner =>
                BuildInteractionAction(),

            // Shelter spots are for resting
            LocationSpotTypes.BasicShelter or
            LocationSpotTypes.CozyShelter or
            LocationSpotTypes.StorageRoom =>
                BuildRestAction(),

            _ => throw new ArgumentException($"Unknown spot type: {spotName}")
        };

        LocationSpot locationSpot = new LocationSpot(spotName, locationName, action);
        return locationSpot;
    }

    private BasicAction BuildProcessingAction()
    {
        var builder = new BasicActionDefinitionBuilder()
            .ForAction(BasicActionTypes.Labor)
            .WithDescription($"Process {inputResource} into {outputResource}");

        // Processing actions can have any costs/rewards except energy rewards
        if (coinCost > 0)
            builder.ExpendsCoins(coinCost);
        if (coinReward > 0)
            builder.RewardsCoins(coinReward);
        if (inputResource.HasValue)
            builder.ExpendsItem(inputResource.Value, inputAmount);
        if (outputResource.HasValue)
            builder.RewardsItem(outputResource.Value, outputAmount);

        // All non-social actions must consume energy
        if (physicalEnergyCost > 0)
            builder.ExpendsEnergy(physicalEnergyCost, EnergyTypes.Physical);
        if (focusEnergyCost > 0)
            builder.ExpendsEnergy(focusEnergyCost, EnergyTypes.Focus);
        if (socialEnergyCost > 0)
            builder.ExpendsEnergy(socialEnergyCost, EnergyTypes.Social);

        // Calculate inventory impact
        int requiredSlots = 0;
        if (outputResource.HasValue)
            requiredSlots += outputAmount;
        if (inputResource.HasValue)
            requiredSlots -= inputAmount;
        if (requiredSlots > 0)
            builder.RequiresInventorySlots(requiredSlots);

        return builder.Build();
    }

    private BasicAction BuildGatheringAction()
    {
        var builder = new BasicActionDefinitionBuilder()
            .ForAction(BasicActionTypes.Gather)
            .WithDescription($"Gather {outputResource}");

        // Handle all possible costs/rewards except energy rewards
        if (coinCost > 0)
            builder.ExpendsCoins(coinCost);
        if (coinReward > 0)
            builder.RewardsCoins(coinReward);
        if (inputResource.HasValue)
            builder.ExpendsItem(inputResource.Value, inputAmount);
        if (outputResource.HasValue)
            builder.RewardsItem(outputResource.Value, outputAmount);

        // Must consume energy
        if (physicalEnergyCost > 0)
            builder.ExpendsEnergy(physicalEnergyCost, EnergyTypes.Physical);
        if (focusEnergyCost > 0)
            builder.ExpendsEnergy(focusEnergyCost, EnergyTypes.Focus);
        if (socialEnergyCost > 0)
            builder.ExpendsEnergy(socialEnergyCost, EnergyTypes.Social);

        // Calculate required inventory slots
        if (outputAmount > 0)
            builder.RequiresInventorySlots(outputAmount);

        return builder.Build();
    }

    private BasicAction BuildTradingAction()
    {
        var builder = new BasicActionDefinitionBuilder()
            .ForAction(BasicActionTypes.Trade);

        if (coinCost > 0)
        {
            builder.WithDescription($"Buy at {spotName}")
                .ExpendsCoins(coinCost);
        }
        else
        {
            builder.WithDescription($"Sell at {spotName}")
                .RewardsCoins(coinReward);
        }

        // Handle any item costs/rewards
        if (inputResource.HasValue)
            builder.ExpendsItem(inputResource.Value, inputAmount);
        if (outputResource.HasValue)
            builder.RewardsItem(outputResource.Value, outputAmount);

        // Must consume energy
        if (physicalEnergyCost > 0)
            builder.ExpendsEnergy(physicalEnergyCost, EnergyTypes.Physical);
        if (focusEnergyCost > 0)
            builder.ExpendsEnergy(focusEnergyCost, EnergyTypes.Focus);
        if (socialEnergyCost > 0)
            builder.ExpendsEnergy(socialEnergyCost, EnergyTypes.Social);

        // Calculate required slots for buying
        if (outputResource.HasValue)
            builder.RequiresInventorySlots(outputAmount);

        return builder.Build();
    }

    private BasicAction BuildInteractionAction()
    {
        var builder = new BasicActionDefinitionBuilder()
            .ForAction(spotName == LocationSpotTypes.PrivateCorner ? BasicActionTypes.Investigate : BasicActionTypes.Mingle)
            .WithDescription(GetInteractionDescription());

        // Social actions can have any costs/rewards INCLUDING energy rewards
        if (coinCost > 0)
            builder.ExpendsCoins(coinCost);
        if (coinReward > 0)
            builder.RewardsCoins(coinReward);
        if (inputResource.HasValue)
            builder.ExpendsItem(inputResource.Value, inputAmount);
        if (outputResource.HasValue)
            builder.RewardsItem(outputResource.Value, outputAmount);

        // Can both consume AND reward energy
        if (physicalEnergyCost > 0)
            builder.ExpendsEnergy(physicalEnergyCost, EnergyTypes.Physical);
        if (focusEnergyCost > 0)
            builder.ExpendsEnergy(focusEnergyCost, EnergyTypes.Focus);
        if (socialEnergyCost > 0)
            builder.ExpendsEnergy(socialEnergyCost, EnergyTypes.Social);

        if (physicalEnergyReward > 0)
            builder.RewardsEnergy(physicalEnergyReward, EnergyTypes.Physical);
        if (focusEnergyReward > 0)
            builder.RewardsEnergy(focusEnergyReward, EnergyTypes.Focus);
        if (socialEnergyReward > 0)
            builder.RewardsEnergy(socialEnergyReward, EnergyTypes.Social);

        // Calculate inventory impact if needed
        if (outputAmount > inputAmount)
            builder.RequiresInventorySlots(outputAmount - inputAmount);

        return builder.Build();
    }

    private BasicAction BuildRestAction()
    {
        // Start by setting up the basic requirements for any rest
        var builder = new BasicActionDefinitionBuilder()
            .ForAction(BasicActionTypes.Rest)
            .WithDescription(GetRestDescription())
            .ExpendsFood(1)         // You need food to recover energy while sleeping
            .AddTimeWindow(TimeWindows.Evening)
            .AddTimeWindow(TimeWindows.Night);

        // If this rest spot has a cost (like an inn room), add it
        if (coinCost > 0)
            builder.ExpendsCoins(coinCost);

        builder.RewardsEnergy(physicalEnergyReward, EnergyTypes.Physical)
               .RewardsEnergy(focusEnergyReward, EnergyTypes.Focus)
               .RewardsEnergy(socialEnergyReward, EnergyTypes.Social);

        return builder.Build();
    }

    private string GetInteractionDescription() => spotName switch
    {
        LocationSpotTypes.PrivateCorner => "Observe quietly",
        LocationSpotTypes.CommonArea => "Socialize with patrons",
        LocationSpotTypes.ServingArea => "Help serve customers",
        _ => "Interact"
    };

    private string GetRestDescription() => spotName switch
    {
        LocationSpotTypes.BasicShelter => "Rest in basic shelter",
        LocationSpotTypes.StorageRoom => "Rest in storage room",
        _ => "Rest"
    };

}
