public class LocationSpotBuilder
{
    private LocationSpotTypes featureType;
    private ResourceTypes? inputResource;
    private ResourceTypes? outputResource;
    private int inputAmount = 0;
    private int outputAmount = 0;
    private int coinCost = 0;
    private int coinReward = 0;
    private int energyCost = 0;
    private EnergyTypes energyType;

    public LocationSpotBuilder ForFeatureType(LocationSpotTypes featureType)
    {
        this.featureType = featureType;
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
        energyCost = amount;
        energyType = type;
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

    private void ValidateFeatureConfiguration()
    {
        switch (featureType)
        {
            case LocationSpotTypes.WoodworkBench:
            case LocationSpotTypes.SmithyForge:
                if (inputResource == null || outputResource == null || inputAmount <= 0 || outputAmount <= 0 || energyCost <= 0 || energyType == default)
                {
                    throw new InvalidOperationException($"Processing feature '{featureType}' is not fully configured.");
                }
                break;

            case LocationSpotTypes.GeneralStore:
            case LocationSpotTypes.ResourceMarket:
            case LocationSpotTypes.SpecialtyShop:
                // Ensure only one of coinCost or coinReward is set
                if (!((coinCost > 0 && coinReward == 0) || (coinCost == 0 && coinReward > 0)))
                {
                    throw new InvalidOperationException($"Trading feature '{featureType}' must have either coinCost or coinReward set, but not both.");
                }
                if (energyCost <= 0 || energyType == default)
                {
                    throw new InvalidOperationException($"Trading feature '{featureType}' is missing energy configuration.");
                }
                if (coinCost > 0 && (outputResource == null || outputAmount <= 0))
                {
                    throw new InvalidOperationException($"Trading feature '{featureType}' is missing output configuration.");
                }
                if (coinReward > 0 && (inputResource == null || inputAmount <= 0))
                {
                    throw new InvalidOperationException($"Trading feature '{featureType}' is missing input configuration");
                }
                break;

            case LocationSpotTypes.ForestGrove:
                if (outputResource == null || outputAmount <= 0 || energyCost <= 0 || energyType == default)
                {
                    throw new InvalidOperationException($"Gathering feature '{featureType}' is not fully configured.");
                }
                break;

            case LocationSpotTypes.BasicShelter:
            case LocationSpotTypes.CozyShelter:
                if (coinCost < 0)
                {
                    throw new InvalidOperationException($"Shelter feature '{featureType}' must have a non-negative coin cost.");
                }
                break;

            default:
                throw new ArgumentException($"Unknown feature type: {featureType}");
        }
    }

    public LocationSpot Build()
    {
        ValidateFeatureConfiguration(); // Add this validation call

        BasicAction action = featureType switch
        {
            LocationSpotTypes.WoodworkBench or
            LocationSpotTypes.SmithyForge =>
                BuildProcessingAction(),

            LocationSpotTypes.GeneralStore or
            LocationSpotTypes.ResourceMarket or
            LocationSpotTypes.SpecialtyShop =>
                BuildTradingAction(),

            LocationSpotTypes.ForestGrove =>
                BuildGatheringAction(),

            LocationSpotTypes.BasicShelter =>
                BuildShelterAction("Rest in basic shelter", 1, 0, 0),

            LocationSpotTypes.CozyShelter =>
                BuildShelterAction("Rest in cozy shelter", 1, 3, 3),

            _ => throw new ArgumentException($"Unknown feature type: {featureType}")
        };

        return new LocationSpot(featureType, action);
    }

    private BasicAction BuildProcessingAction()
    {
        return new BasicActionDefinitionBuilder()
            .ForAction(BasicActionTypes.Labor)
            .WithDescription($"Process {inputResource} into {outputResource}")
            .ExpendsEnergy(energyCost, energyType)
            .ExpendsItem(inputResource.Value, inputAmount)
            .RewardsItem(outputResource.Value, outputAmount)
            .RequiresInventorySlots(1)
            .Build();
    }

    private BasicAction BuildTradingAction()
    {
        if (coinCost > 0)
        {
            return new BasicActionDefinitionBuilder()
                .ForAction(BasicActionTypes.Trade)
                .WithDescription($"Buy at {featureType}")
                .ExpendsEnergy(energyCost, energyType)
                .ExpendsCoins(coinCost)
                .RewardsItem(outputResource.Value, outputAmount)
                .RequiresInventorySlots(1)
                .Build();
        }
        else if (coinReward > 0)
        {
            return new BasicActionDefinitionBuilder()
                .ForAction(BasicActionTypes.Trade)
                .WithDescription($"Sell at {featureType}")
                .ExpendsEnergy(energyCost, energyType)
                .ExpendsItem(inputResource.Value, inputAmount)
                .RewardsCoins(coinReward)
                .Build();
        }
        throw new ArgumentException($"Trading feature {featureType} must specify buying or selling prices");
    }

    private BasicAction BuildGatheringAction()
    {
        return new BasicActionDefinitionBuilder()
            .ForAction(BasicActionTypes.Gather)
            .WithDescription($"Gather {outputResource}")
            .ExpendsEnergy(energyCost, energyType)
            .RewardsItem(outputResource.Value, outputAmount)
            .RequiresInventorySlots(1)
            .Build();
    }

    private BasicAction BuildShelterAction(string description, int foodCost, int physicalEnergyReward, int focusEnergyReward)
    {
        return new BasicActionDefinitionBuilder()
            .ForAction(BasicActionTypes.Rest)
            .WithDescription(description)
            .ExpendsFood(foodCost)
            .ExpendsCoins(coinCost)
            .RewardsPhysicalEnergy(physicalEnergyReward)
            .RewardsFocusEnergy(focusEnergyReward)
            .RewardsSocialEnergy(focusEnergyReward)
            .AddTimeWindow(TimeWindows.Night)
            .Build();
    }

}
