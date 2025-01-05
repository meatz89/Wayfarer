public enum LocationSpotNames
{
    // Social
    TavernBarterTable,
    CellarPantry,
    TavernKitchen,
    InnFireplace,
    MarketSquare,
    MarketBazaar,
    MarketPorters,
    HerbGarden,

    // Nature
    Road,
    Forest,
    MysticGrove,
    LumberYard,
    WoodworkerCabin,
    GroveShrine,
    Field,
    Campground,
    Cave,
    Cavern,
    UndergroundLake,
    HuntingGrounds,
    FishingSpot,
    GatheringSpot,

    // Industrial
    FishingWharf,
    WharfMerchant,
    DocksidePub,
    Warehouse,
    DocksideWarehouse,
    Factory,
    Workshop,

    // Commercial
    Market,
    Shop,
    Garden,
    TradingPost,
    MerchantStand,
    TravelerLodge,

    Undefined,
    TavernRoom,
    DockArea,
    PublicMarket,
    TavernInvestigate,
    TavernMingle
}

public static class LocationSpotMapper
{
    public static LocationSpotNames GetLocationSpot(LocationTypes locationType, BasicActionTypes baseAction, ExposureConditions exposure, ScaleVariations scale)
    {
        return locationType switch
        {
            LocationTypes.Social => GetSocialSpot(baseAction, exposure),
            LocationTypes.Nature => GetNatureSpot(baseAction, exposure, scale),
            LocationTypes.Industrial => GetIndustrialSpot(baseAction, exposure, scale),
            LocationTypes.Commercial => GetCommercialSpot(baseAction, exposure),
            _ => LocationSpotNames.Undefined
        };
    }

    private static LocationSpotNames GetSocialSpot(BasicActionTypes baseAction, ExposureConditions exposure)
    {
        if (exposure == ExposureConditions.Indoor)
        {
            return baseAction switch
            {
                BasicActionTypes.Labor => LocationSpotNames.TavernKitchen,
                BasicActionTypes.Gather => LocationSpotNames.CellarPantry,
                BasicActionTypes.Trade => LocationSpotNames.TavernBarterTable,
                BasicActionTypes.Mingle => LocationSpotNames.TavernMingle,
                BasicActionTypes.Perform => LocationSpotNames.InnFireplace,
                BasicActionTypes.Investigate => LocationSpotNames.TavernInvestigate,
                _ => LocationSpotNames.TavernRoom
            };
        }

        return baseAction switch
        {
            BasicActionTypes.Labor => LocationSpotNames.MarketPorters,
            BasicActionTypes.Gather => LocationSpotNames.HerbGarden,
            BasicActionTypes.Trade => LocationSpotNames.MarketBazaar,
            BasicActionTypes.Mingle => LocationSpotNames.MarketSquare,
            _ => LocationSpotNames.PublicMarket
        };
    }

    private static LocationSpotNames GetNatureSpot(BasicActionTypes baseAction, ExposureConditions exposure, ScaleVariations scale)
    {
        if (exposure == ExposureConditions.Indoor)
        {
            return scale switch
            {
                ScaleVariations.Intimate => LocationSpotNames.Cave,
                ScaleVariations.Medium => LocationSpotNames.Cavern,
                _ => LocationSpotNames.UndergroundLake
            };
        }

        return scale switch
        {
            ScaleVariations.Intimate => baseAction switch
            {
                BasicActionTypes.Trade => LocationSpotNames.WoodworkerCabin,
                BasicActionTypes.Mingle => LocationSpotNames.GroveShrine,
                _ => LocationSpotNames.Road
            },
            ScaleVariations.Medium => baseAction switch
            {
                BasicActionTypes.Labor => LocationSpotNames.LumberYard,
                BasicActionTypes.Gather => LocationSpotNames.MysticGrove,
                _ => LocationSpotNames.Forest
            },
            _ => LocationSpotNames.Field
        };
    }

    private static LocationSpotNames GetIndustrialSpot(BasicActionTypes baseAction, ExposureConditions exposure, ScaleVariations scale)
    {
        if (exposure == ExposureConditions.Indoor)
        {
            if (scale == ScaleVariations.Large)
            {
                return baseAction == BasicActionTypes.Labor
                    ? LocationSpotNames.DocksideWarehouse
                    : LocationSpotNames.Warehouse;
            }

            return scale == ScaleVariations.Medium
                ? LocationSpotNames.Factory
                : LocationSpotNames.Workshop;
        }

        return baseAction switch
        {
            BasicActionTypes.Gather => LocationSpotNames.FishingWharf,
            BasicActionTypes.Trade => LocationSpotNames.WharfMerchant,
            BasicActionTypes.Mingle => LocationSpotNames.DocksidePub,
            _ => LocationSpotNames.DockArea
        };
    }

    private static LocationSpotNames GetCommercialSpot(BasicActionTypes baseAction, ExposureConditions exposure)
    {
        if (exposure == ExposureConditions.Indoor)
        {
            return LocationSpotNames.Shop;
        }

        return baseAction switch
        {
            BasicActionTypes.Labor => LocationSpotNames.MarketPorters,
            BasicActionTypes.Gather => LocationSpotNames.HerbGarden,
            BasicActionTypes.Trade => LocationSpotNames.MarketBazaar,
            BasicActionTypes.Mingle => LocationSpotNames.MarketSquare,
            _ => LocationSpotNames.Market
        };
    }
}