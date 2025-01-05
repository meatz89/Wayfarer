public enum LocationSpotNames
{
    // Social Indoor
    Tavern,
    TavernBarterTable,
    CellarPantry,
    TavernKitchen,
    InnFireplace,
    TavernStudy, // Added for Study
    TavernRest, // Added for Rest

    // Social Outdoor
    PublicMarket,
    MarketSquare, // For Mingle
    MarketBazaar, // For Trade
    MarketPorters, // For Labor
    HerbGarden, // For Gather
    MarketPerformance, // For Perform

    // Nature Outdoor Intimate
    Road,
    WoodworkerCabin, // For Trade
    GroveShrine, // For Mingle
    RoadsideCamp, // For Rest

    // Nature Outdoor Medium
    Forest,
    MysticGrove, // For Gather
    LumberYard, // For Labor
    HuntingGrounds, // For Gather (Hunt)

    // Nature Outdoor Large
    Field,
    Campground,

    // Nature Indoor
    Cave,
    Cavern,
    UndergroundLake,

    // Industrial Outdoor
    Dock,
    FishingWharf,
    WharfMerchant,
    DocksidePub,

    // Industrial Indoor Large
    Warehouse,
    DocksideWarehouse, // For Labor

    // Industrial Indoor Medium
    Factory,

    // Industrial Indoor Intimate
    Workshop,

    // Commercial Outdoor
    Market,
    Garden,
    TradingPost, // For Trade
    MerchantStand, // For Trade (alternative)

    // Commercial Indoor
    Shop,

    // TravelerLodge (Moved from Nature)
    TravelerLodge,

    Undefined,
    TavernRoom
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
            LocationTypes.Commercial => GetCommercialSpot(baseAction, exposure, scale),
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
                BasicActionTypes.Mingle => LocationSpotNames.Tavern,
                BasicActionTypes.Perform => LocationSpotNames.InnFireplace,
                BasicActionTypes.Investigate => LocationSpotNames.Tavern,
                BasicActionTypes.Study => LocationSpotNames.TavernStudy,
                BasicActionTypes.Rest => LocationSpotNames.TavernRoom,
                _ => LocationSpotNames.Tavern
            };
        }

        // Outdoor
        return baseAction switch
        {
            BasicActionTypes.Labor => LocationSpotNames.MarketPorters,
            BasicActionTypes.Gather => LocationSpotNames.HerbGarden,
            BasicActionTypes.Trade => LocationSpotNames.MarketBazaar,
            BasicActionTypes.Mingle => LocationSpotNames.MarketSquare,
            BasicActionTypes.Perform => LocationSpotNames.MarketPerformance,
            _ => LocationSpotNames.PublicMarket
        };
    }

    private static LocationSpotNames GetNatureSpot(BasicActionTypes baseAction, ExposureConditions exposure, ScaleVariations scale)
    {
        if (exposure == ExposureConditions.Outdoor)
        {
            switch (scale)
            {
                case ScaleVariations.Intimate:
                    return baseAction switch
                    {
                        BasicActionTypes.Trade => LocationSpotNames.WoodworkerCabin,
                        BasicActionTypes.Mingle => LocationSpotNames.GroveShrine,
                        BasicActionTypes.Rest => LocationSpotNames.RoadsideCamp,
                        _ => LocationSpotNames.Road
                    };
                case ScaleVariations.Medium:
                    return baseAction switch
                    {
                        BasicActionTypes.Labor => LocationSpotNames.LumberYard,
                        BasicActionTypes.Gather => LocationSpotNames.Forest,
                        BasicActionTypes.Perform => LocationSpotNames.MysticGrove,
                        _ => LocationSpotNames.Forest
                    };
                case ScaleVariations.Large:
                    return baseAction switch
                    {
                        BasicActionTypes.Rest => LocationSpotNames.Campground,
                        _ => LocationSpotNames.Field
                    };
            }
        }
        else // Indoor
        {
            return scale switch
            {
                ScaleVariations.Intimate => LocationSpotNames.Cave,
                ScaleVariations.Medium => LocationSpotNames.Cavern,
                ScaleVariations.Large => LocationSpotNames.UndergroundLake,
                _ => LocationSpotNames.Cave // Default
            };
        }

        return LocationSpotNames.Undefined; // Fallback
    }

    private static LocationSpotNames GetIndustrialSpot(BasicActionTypes baseAction, ExposureConditions exposure, ScaleVariations scale)
    {
        if (exposure == ExposureConditions.Outdoor)
        {
            return baseAction switch
            {
                BasicActionTypes.Gather => LocationSpotNames.FishingWharf,
                BasicActionTypes.Trade => LocationSpotNames.WharfMerchant,
                BasicActionTypes.Mingle => LocationSpotNames.DocksidePub,
                _ => LocationSpotNames.Dock
            };
        }
        else // Indoor
        {
            switch (scale)
            {
                case ScaleVariations.Large:
                    return baseAction switch
                    {
                        BasicActionTypes.Labor => LocationSpotNames.DocksideWarehouse,
                        _ => LocationSpotNames.Warehouse
                    };
                case ScaleVariations.Medium:
                    return LocationSpotNames.Factory;
                case ScaleVariations.Intimate:
                default:
                    return LocationSpotNames.Workshop;
            }
        }
    }

    private static LocationSpotNames GetCommercialSpot(BasicActionTypes baseAction, ExposureConditions exposure, ScaleVariations scale)
    {
        if (exposure == ExposureConditions.Outdoor)
        {
            return baseAction switch
            {
                BasicActionTypes.Gather => LocationSpotNames.HerbGarden,
                BasicActionTypes.Trade => LocationSpotNames.MarketBazaar,
                BasicActionTypes.Mingle => LocationSpotNames.MarketSquare,
                BasicActionTypes.Labor => LocationSpotNames.MarketPorters,
                _ => LocationSpotNames.Market
            };
        }
        else // Indoor
        {
            return baseAction switch
            {
                BasicActionTypes.Study => LocationSpotNames.Workshop,
                _ => LocationSpotNames.Shop
            };
        }
    }
}