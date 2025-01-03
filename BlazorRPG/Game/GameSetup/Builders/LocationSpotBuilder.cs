public class LocationSpotBuilder
{
    private LocationNames locationName;
    private LocationSpotNames spotName;
    private CharacterNames character;
    private ResourceTypes? inputResource;
    private ResourceTypes? outputResource;
    private AccessTypes accessType;

    private int inputAmount = 0;
    private int outputAmount = 0;
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
        switch (type)
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
        BasicActionDefinitionBuilder builder = new BasicActionDefinitionBuilder()
            .ForAction(BasicActionTypes.Discuss)
            .WithDescription($"Talk to {character}");

        return builder.Build();
    }

    private BasicAction BuildProcessingAction()
    {
        BasicActionDefinitionBuilder builder = new BasicActionDefinitionBuilder()
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
            builder.RewardsResource(outputResource.Value, outputAmount);

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
        BasicActionDefinitionBuilder builder = new BasicActionDefinitionBuilder()
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
            builder.RewardsResource(outputResource.Value, outputAmount);

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
        BasicActionDefinitionBuilder builder = new BasicActionDefinitionBuilder()
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
            builder.RewardsResource(outputResource.Value, outputAmount);

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
        BasicActionDefinitionBuilder builder = new BasicActionDefinitionBuilder()
            .ForAction(spotName == LocationSpotNames.PrivateCorner ? BasicActionTypes.Investigate : BasicActionTypes.Mingle)
            .WithDescription(GetInteractionDescription());

        // Social actions can have any costs/rewards INCLUDING energy rewards
        if (coinCost > 0)
            builder.ExpendsCoins(coinCost);
        if (coinReward > 0)
            builder.RewardsCoins(coinReward);
        if (inputResource.HasValue)
            builder.ExpendsItem(inputResource.Value, inputAmount);
        if (outputResource.HasValue)
            builder.RewardsResource(outputResource.Value, outputAmount);

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
        return new BasicAction
        {
            ActionType = BasicActionTypes.Rest,
            Name = GetRestDescription(),
            TimeSlots = new List<TimeWindows> { TimeWindows.Evening, TimeWindows.Night },
            Requirements = new List<Requirement>
            {
                new ResourceRequirement(ResourceTypes.Food, 1)
            },
                Costs = new List<Outcome>()
            {
                new ResourceOutcome(ResourceTypes.Food, -1),
                new DayChangeOutcome() 
            },
                Rewards = new List<Outcome>
            {
                new EnergyOutcome(EnergyTypes.Physical, 5),
                new EnergyOutcome(EnergyTypes.Focus, 5),
                new EnergyOutcome(EnergyTypes.Social, 5),
                new DayChangeOutcome()
            }
        };
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
                BuildProcessingAction(),

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
