public class LocationSpotBuilder
{
    private LocationNames locationName;
    private LocationSpotNames spotName;
    private CharacterNames character;
    private ResourceTypes? inputResource;
    private ResourceTypes? outputResource;
    private AccessTypes accessType;

    private int inputCount = 0;
    private int outputCount = 0;
    private int coinCost = 0;
    private int coinReward = 0;

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

    public LocationSpotBuilder ForLocationSpot(LocationSpotNames featureType)
    {
        this.spotName = featureType;
        return this;
    }

    public LocationSpotBuilder WithCharacter(CharacterNames character)
    {
        this.character = character;
        return this;
    }

    public LocationSpotBuilder WithInputResource(ResourceTypes resource, int count)
    {
        inputResource = resource;
        inputCount = count;
        return this;
    }

    public LocationSpotBuilder WithOutputResource(ResourceTypes resource, int count)
    {
        outputResource = resource;
        outputCount = count;
        return this;
    }

    public LocationSpotBuilder WithEnergyCost(int count, EnergyTypes type)
    {
        switch (type)
        {
            case EnergyTypes.Physical:
                physicalEnergyCost = count;
                break;

            case EnergyTypes.Focus:
                focusEnergyCost = count;
                break;

            case EnergyTypes.Social:
                socialEnergyCost = count;
                break;
        }
        return this;
    }

    public LocationSpotBuilder WithEnergyReward(int count, EnergyTypes type)
    {
        switch (type)
        {
            case EnergyTypes.Physical:
                physicalEnergyReward = count;
                break;

            case EnergyTypes.Focus:
                focusEnergyReward = count;
                break;

            case EnergyTypes.Social:
                socialEnergyReward = count;
                break;
        }
        return this;
    }

    public LocationSpotBuilder WithCoinCost(int coinsToInvest)
    {
        coinCost = coinsToInvest;
        return this;
    }

    public LocationSpotBuilder WithCoinReward(int coinsToReceive)
    {
        coinReward = coinsToReceive;
        return this;
    }

    public LocationSpotBuilder SetAccessType(AccessTypes accessType)
    {
        this.accessType = accessType;
        return this;
    }


    private BasicAction BuildCharacterAction(CharacterNames character)
    {
        ActionBuilder builder = new ActionBuilder()
            .ForAction(ActionTypes.Discuss)
            .WithDescription($"Talk to {character}");

        return builder.Build();
    }

    private BasicAction BuildLaborAction()
    {
        ActionBuilder builder = new ActionBuilder()
            .ForAction(ActionTypes.Labor)
            .WithDescription($"Process {inputResource} into {outputResource}");

        // Processing actions can have any costs/rewards except energy rewards
        if (coinCost > 0)
            builder.ExpendsCoins(coinCost);
        if (coinReward > 0)
            builder.RewardsCoins(coinReward);
        if (inputResource.HasValue)
            builder.ExpendsItem(inputResource.Value, inputCount);
        if (outputResource.HasValue)
            builder.RewardsResource(outputResource.Value, outputCount);

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
            requiredSlots += outputCount;
        if (inputResource.HasValue)
            requiredSlots -= inputCount;
        if (requiredSlots > 0)
            builder.RequiresInventorySlots(requiredSlots);

        return builder.Build();
    }

    private BasicAction BuildGatheringAction()
    {
        ActionBuilder builder = new ActionBuilder()
            .ForAction(ActionTypes.Gather)
            .WithDescription($"Gather {outputResource}");

        // Handle all possible costs/rewards except energy rewards
        if (coinCost > 0)
            builder.ExpendsCoins(coinCost);
        if (coinReward > 0)
            builder.RewardsCoins(coinReward);
        if (inputResource.HasValue)
            builder.ExpendsItem(inputResource.Value, inputCount);
        if (outputResource.HasValue)
            builder.RewardsResource(outputResource.Value, outputCount);

        // Must consume energy
        if (physicalEnergyCost > 0)
            builder.ExpendsEnergy(physicalEnergyCost, EnergyTypes.Physical);
        if (focusEnergyCost > 0)
            builder.ExpendsEnergy(focusEnergyCost, EnergyTypes.Focus);
        if (socialEnergyCost > 0)
            builder.ExpendsEnergy(socialEnergyCost, EnergyTypes.Social);

        // Calculate required inventory slots
        if (outputCount > 0)
            builder.RequiresInventorySlots(outputCount);

        return builder.Build();
    }

    private BasicAction BuildTradingAction()
    {
        ActionBuilder builder = new ActionBuilder()
            .ForAction(ActionTypes.Trade);

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
            builder.ExpendsItem(inputResource.Value, inputCount);
        if (outputResource.HasValue)
            builder.RewardsResource(outputResource.Value, outputCount);

        // Must consume energy
        if (physicalEnergyCost > 0)
            builder.ExpendsEnergy(physicalEnergyCost, EnergyTypes.Physical);
        if (focusEnergyCost > 0)
            builder.ExpendsEnergy(focusEnergyCost, EnergyTypes.Focus);
        if (socialEnergyCost > 0)
            builder.ExpendsEnergy(socialEnergyCost, EnergyTypes.Social);

        // Calculate required slots for buying
        if (outputResource.HasValue)
            builder.RequiresInventorySlots(outputCount);

        return builder.Build();
    }

    private BasicAction BuildInteractionAction()
    {
        ActionBuilder builder = new ActionBuilder()
            .ForAction(spotName == LocationSpotNames.PrivateCorner ? ActionTypes.Investigate : ActionTypes.Mingle)
            .WithDescription(GetInteractionDescription());

        // Social actions can have any costs/rewards INCLUDING energy rewards
        if (coinCost > 0)
            builder.ExpendsCoins(coinCost);
        if (coinReward > 0)
            builder.RewardsCoins(coinReward);
        if (inputResource.HasValue)
            builder.ExpendsItem(inputResource.Value, inputCount);
        if (outputResource.HasValue)
            builder.RewardsResource(outputResource.Value, outputCount);

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
        if (outputCount > inputCount)
            builder.RequiresInventorySlots(outputCount - inputCount);

        return builder.Build();
    }

    private BasicAction BuildRestAction()
    {
        ActionBuilder builder = new ActionBuilder()
            .ForAction(ActionTypes.Rest);

        builder.WithDescription(GetRestDescription())
        .AddTimeSlot(TimeSlots.Night)
        .WithTimeInvestment(0)
        .ExpendsFood(1)
        .RewardsEnergy(5, EnergyTypes.Physical)
        .RewardsEnergy(5, EnergyTypes.Focus)
        .RewardsEnergy(5, EnergyTypes.Social)
        .EndsDay();

        return builder.Build();
    }

    private string GetInteractionDescription() => spotName switch
    {
        LocationSpotNames.PrivateCorner => "Observe quietly",
        LocationSpotNames.CommonArea => "Socialize with patrons",
        LocationSpotNames.ServingArea => "Help serve customers",
        _ => "Interact"
    };

    private string GetRestDescription() => spotName switch
    {
        LocationSpotNames.BasicShelter => "Rest in basic shelter",
        LocationSpotNames.GoodShelter => "Rest in storage room",
        _ => "Rest"
    };

    public LocationSpot Build()
    {
        // Each spot type determines what kind of action it supports
        BasicAction spotAction = spotName switch
        {
            // Processing spots support labor actions that convert resources
            LocationSpotNames.WoodworkBench or
            LocationSpotNames.SmithyForge or
            LocationSpotNames.TanningRack or
            LocationSpotNames.WeavingLoom =>
                BuildLaborAction(),

            // Trading spots support buying/selling with coins
            LocationSpotNames.GeneralStore or
            LocationSpotNames.ResourceMarket or
            LocationSpotNames.SpecialtyShop or
            LocationSpotNames.TavernBar =>
                BuildTradingAction(),

            // Gathering spots produce resources from environment
            LocationSpotNames.ForestGrove or
            LocationSpotNames.MineralDeposit or
            LocationSpotNames.HuntingSpot =>
                BuildGatheringAction(),

            // Social spots support various interaction types
            LocationSpotNames.CommonArea or
            LocationSpotNames.ServingArea or
            LocationSpotNames.PrivateCorner =>
                BuildInteractionAction(),

            // Shelter spots are for resting
            LocationSpotNames.BasicShelter or
            LocationSpotNames.GoodShelter =>
                BuildRestAction(),

            _ => throw new ArgumentException($"Unknown spot type: {spotName}")
        };

        List<BasicAction> characterActions = new();
        if (character != default)
        {
            characterActions.Add(BuildCharacterAction(character));
        }

        LocationSpot locationSpot = new LocationSpot(
            spotName,
            locationName,
            character,
            spotAction,
            characterActions);

        return locationSpot;
    }
}
