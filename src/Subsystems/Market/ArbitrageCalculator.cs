using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Subsystems.MarketSubsystem
{
    /// <summary>
    /// Calculates arbitrage opportunities between locations.
    /// Identifies profitable trade routes and provides trading insights.
    /// </summary>
    public class ArbitrageCalculator
    {
        private readonly GameWorld _gameWorld;
        private readonly PriceManager _priceManager;
        private readonly ItemRepository _itemRepository;

        public ArbitrageCalculator(
            GameWorld gameWorld,
            PriceManager priceManager,
            ItemRepository itemRepository)
        {
            _gameWorld = gameWorld;
            _priceManager = priceManager;
            _itemRepository = itemRepository;
        }

        /// <summary>
        /// Represents an arbitrage opportunity for trading an item between locations
        /// </summary>
        public class ArbitrageOpportunity
        {
            public string ItemId { get; set; }
            public string ItemName { get; set; }
            public string BuyLocationId { get; set; }
            public string BuyLocationName { get; set; }
            public int BuyPrice { get; set; }
            public string SellLocationId { get; set; }
            public string SellLocationName { get; set; }
            public int SellPrice { get; set; }
            public int GrossProfit { get; set; }
            public int TravelCost { get; set; }
            public int NetProfit { get; set; }
            public float ProfitMargin { get; set; } // Percentage return on investment
            public int RequiredCapital { get; set; }
            public bool IsCurrentlyProfitable { get; set; }
            public string OpportunityDescription { get; set; }
            public int DistanceBetweenLocations { get; set; }
            public float ProfitPerDistance { get; set; }
        }

        /// <summary>
        /// Represents a complete trade route with multiple stops
        /// </summary>
        public class TradeRoute
        {
            public List<string> LocationSequence { get; set; } = new List<string>();
            public List<ArbitrageOpportunity> Trades { get; set; } = new List<ArbitrageOpportunity>();
            public int TotalProfit { get; set; }
            public int RequiredCapital { get; set; }
            public int TotalDistance { get; set; }
            public float AverageProfitMargin { get; set; }
            public string RouteDescription { get; set; }
        }

        // ========== ARBITRAGE CALCULATION ==========

        /// <summary>
        /// Find the best arbitrage opportunity for a specific item
        /// </summary>
        public ArbitrageOpportunity FindBestOpportunity(string itemId)
        {
            Item item = _itemRepository.GetItemById(itemId);
            if (item == null) return null;

            List<Location> locations = _gameWorld.WorldState.locations ?? new List<Location>();
            ArbitrageOpportunity bestOpportunity = null;
            int highestProfit = 0;

            // Compare all location pairs
            foreach (Location buyLocation in locations)
            {
                int buyPrice = _priceManager.GetBuyPrice(itemId, buyLocation.Id);
                if (buyPrice <= 0) continue; // Item not available for purchase here

                foreach (Location sellLocation in locations)
                {
                    if (buyLocation.Id == sellLocation.Id) continue; // Skip same location

                    int sellPrice = _priceManager.GetSellPrice(itemId, sellLocation.Id);
                    if (sellPrice <= 0) continue; // Can't sell here

                    int grossProfit = sellPrice - buyPrice;
                    if (grossProfit <= 0) continue; // Not profitable

                    // Calculate travel cost (simplified - could be enhanced)
                    int distance = CalculateDistance(buyLocation.Id, sellLocation.Id);
                    int travelCost = CalculateTravelCost(distance);
                    int netProfit = grossProfit - travelCost;

                    if (netProfit > highestProfit)
                    {
                        highestProfit = netProfit;
                        bestOpportunity = new ArbitrageOpportunity
                        {
                            ItemId = itemId,
                            ItemName = item.Name,
                            BuyLocationId = buyLocation.Id,
                            BuyLocationName = buyLocation.Name,
                            BuyPrice = buyPrice,
                            SellLocationId = sellLocation.Id,
                            SellLocationName = sellLocation.Name,
                            SellPrice = sellPrice,
                            GrossProfit = grossProfit,
                            TravelCost = travelCost,
                            NetProfit = netProfit,
                            ProfitMargin = (float)netProfit / buyPrice,
                            RequiredCapital = buyPrice,
                            IsCurrentlyProfitable = netProfit > 0,
                            DistanceBetweenLocations = distance,
                            ProfitPerDistance = distance > 0 ? (float)netProfit / distance : netProfit,
                            OpportunityDescription = GenerateOpportunityDescription(item.Name, buyLocation.Name, sellLocation.Name, netProfit)
                        };
                    }
                }
            }

            return bestOpportunity;
        }

        /// <summary>
        /// Find all profitable arbitrage opportunities
        /// </summary>
        public List<ArbitrageOpportunity> FindAllOpportunities()
        {
            List<ArbitrageOpportunity> opportunities = new List<ArbitrageOpportunity>();
            List<Item> allItems = _itemRepository.GetAllItems();

            foreach (Item item in allItems)
            {
                ArbitrageOpportunity opportunity = FindBestOpportunity(item.Id);
                if (opportunity != null && opportunity.IsCurrentlyProfitable)
                {
                    opportunities.Add(opportunity);
                }
            }

            // Sort by net profit descending
            return opportunities.OrderByDescending(o => o.NetProfit).ToList();
        }

        /// <summary>
        /// Calculate profit for a specific trade between two locations
        /// </summary>
        public int CalculateProfit(string itemId, string buyLocationId, string sellLocationId)
        {
            int buyPrice = _priceManager.GetBuyPrice(itemId, buyLocationId);
            int sellPrice = _priceManager.GetSellPrice(itemId, sellLocationId);

            if (buyPrice <= 0 || sellPrice <= 0) return -1;

            int distance = CalculateDistance(buyLocationId, sellLocationId);
            int travelCost = CalculateTravelCost(distance);

            return sellPrice - buyPrice - travelCost;
        }

        // ========== ADVANCED ARBITRAGE ANALYSIS ==========

        /// <summary>
        /// Find arbitrage opportunities the player can afford
        /// </summary>
        public List<ArbitrageOpportunity> FindAffordableOpportunities()
        {
            Player player = _gameWorld.GetPlayer();
            int availableCoins = player.Coins;

            return FindAllOpportunities()
                .Where(o => o.RequiredCapital <= availableCoins)
                .ToList();
        }

        /// <summary>
        /// Find arbitrage opportunities from the player's current location
        /// </summary>
        public List<ArbitrageOpportunity> FindOpportunitiesFromCurrentLocation()
        {
            Player player = _gameWorld.GetPlayer();
            string currentLocationId = player.CurrentLocationSpot?.LocationId;
            List<ArbitrageOpportunity> opportunities = new List<ArbitrageOpportunity>();
            List<Item> allItems = _itemRepository.GetAllItems();

            foreach (Item item in allItems)
            {
                int buyPrice = _priceManager.GetBuyPrice(item.Id, currentLocationId);
                if (buyPrice <= 0) continue;

                List<Location> locations = _gameWorld.WorldState.locations ?? new List<Location>();
                foreach (Location sellLocation in locations)
                {
                    if (sellLocation.Id == currentLocationId) continue;

                    int sellPrice = _priceManager.GetSellPrice(item.Id, sellLocation.Id);
                    if (sellPrice <= 0) continue;

                    int distance = CalculateDistance(currentLocationId, sellLocation.Id);
                    int travelCost = CalculateTravelCost(distance);
                    int netProfit = sellPrice - buyPrice - travelCost;

                    if (netProfit > 0)
                    {
                        opportunities.Add(new ArbitrageOpportunity
                        {
                            ItemId = item.Id,
                            ItemName = item.Name,
                            BuyLocationId = currentLocationId,
                            BuyLocationName = GetLocationName(currentLocationId),
                            BuyPrice = buyPrice,
                            SellLocationId = sellLocation.Id,
                            SellLocationName = sellLocation.Name,
                            SellPrice = sellPrice,
                            GrossProfit = sellPrice - buyPrice,
                            TravelCost = travelCost,
                            NetProfit = netProfit,
                            ProfitMargin = (float)netProfit / buyPrice,
                            RequiredCapital = buyPrice,
                            IsCurrentlyProfitable = true,
                            DistanceBetweenLocations = distance,
                            ProfitPerDistance = distance > 0 ? (float)netProfit / distance : netProfit,
                            OpportunityDescription = GenerateOpportunityDescription(item.Name, GetLocationName(currentLocationId), sellLocation.Name, netProfit)
                        });
                    }
                }
            }

            return opportunities.OrderByDescending(o => o.NetProfit).ToList();
        }

        /// <summary>
        /// Find opportunities for items the player already owns
        /// </summary>
        public List<ArbitrageOpportunity> FindOpportunitiesForInventory()
        {
            Player player = _gameWorld.GetPlayer();
            string currentLocationId = player.CurrentLocationSpot?.LocationId;
            List<ArbitrageOpportunity> opportunities = new List<ArbitrageOpportunity>();

            foreach (string itemId in player.Inventory.GetItemIds())
            {
                Item item = _itemRepository.GetItemById(itemId);
                if (item == null) continue;

                int currentSellPrice = _priceManager.GetSellPrice(itemId, currentLocationId);
                List<Location> locations = _gameWorld.WorldState.locations ?? new List<Location>();

                foreach (Location sellLocation in locations)
                {
                    if (sellLocation.Id == currentLocationId) continue;

                    int otherSellPrice = _priceManager.GetSellPrice(itemId, sellLocation.Id);
                    if (otherSellPrice <= currentSellPrice) continue;

                    int distance = CalculateDistance(currentLocationId, sellLocation.Id);
                    int travelCost = CalculateTravelCost(distance);
                    int netProfit = otherSellPrice - currentSellPrice - travelCost;

                    if (netProfit > 0)
                    {
                        opportunities.Add(new ArbitrageOpportunity
                        {
                            ItemId = itemId,
                            ItemName = item.Name,
                            BuyLocationId = currentLocationId,
                            BuyLocationName = GetLocationName(currentLocationId),
                            BuyPrice = currentSellPrice, // What we could sell for here
                            SellLocationId = sellLocation.Id,
                            SellLocationName = sellLocation.Name,
                            SellPrice = otherSellPrice,
                            GrossProfit = otherSellPrice - currentSellPrice,
                            TravelCost = travelCost,
                            NetProfit = netProfit,
                            ProfitMargin = currentSellPrice > 0 ? (float)netProfit / currentSellPrice : 0,
                            RequiredCapital = 0, // Already own the item
                            IsCurrentlyProfitable = true,
                            DistanceBetweenLocations = distance,
                            ProfitPerDistance = distance > 0 ? (float)netProfit / distance : netProfit,
                            OpportunityDescription = $"Sell {item.Name} in {sellLocation.Name} for {netProfit} coin profit"
                        });
                    }
                }
            }

            return opportunities.OrderByDescending(o => o.NetProfit).ToList();
        }

        // ========== TRADE ROUTE PLANNING ==========

        /// <summary>
        /// Plan an optimal trade route visiting multiple locations
        /// </summary>
        public TradeRoute PlanOptimalRoute(int maxStops = 3)
        {
            Player player = _gameWorld.GetPlayer();
            string startLocation = player.CurrentLocationSpot?.LocationId;
            int availableCapital = player.Coins;

            TradeRoute bestRoute = new TradeRoute();
            bestRoute.LocationSequence.Add(startLocation);

            string currentLocation = startLocation;
            int currentCapital = availableCapital;
            int totalProfit = 0;

            for (int stop = 0; stop < maxStops; stop++)
            {
                // Find best opportunity from current location with available capital
                List<ArbitrageOpportunity> opportunities = FindAllOpportunitiesFrom(currentLocation)
                    .Where(o => o.RequiredCapital <= currentCapital)
                    .OrderByDescending(o => o.NetProfit)
                    .ToList();

                if (opportunities.Count == 0) break;

                ArbitrageOpportunity bestOpp = opportunities.First();
                bestRoute.Trades.Add(bestOpp);
                bestRoute.LocationSequence.Add(bestOpp.SellLocationId);

                // Update capital and profit
                currentCapital = currentCapital - bestOpp.BuyPrice + bestOpp.SellPrice - bestOpp.TravelCost;
                totalProfit += bestOpp.NetProfit;
                currentLocation = bestOpp.SellLocationId;
            }

            bestRoute.TotalProfit = totalProfit;
            bestRoute.RequiredCapital = availableCapital;
            bestRoute.TotalDistance = bestRoute.Trades.Sum(t => t.DistanceBetweenLocations);
            bestRoute.AverageProfitMargin = bestRoute.Trades.Count > 0
                ? bestRoute.Trades.Average(t => t.ProfitMargin)
                : 0;
            bestRoute.RouteDescription = GenerateRouteDescription(bestRoute);

            return bestRoute;
        }

        /// <summary>
        /// Find all opportunities from a specific location
        /// </summary>
        private List<ArbitrageOpportunity> FindAllOpportunitiesFrom(string fromLocationId)
        {
            List<ArbitrageOpportunity> opportunities = new List<ArbitrageOpportunity>();
            List<Item> allItems = _itemRepository.GetAllItems();
            List<Location> locations = _gameWorld.WorldState.locations ?? new List<Location>();

            foreach (Item item in allItems)
            {
                int buyPrice = _priceManager.GetBuyPrice(item.Id, fromLocationId);
                if (buyPrice <= 0) continue;

                foreach (Location sellLocation in locations)
                {
                    if (sellLocation.Id == fromLocationId) continue;

                    int sellPrice = _priceManager.GetSellPrice(item.Id, sellLocation.Id);
                    if (sellPrice <= buyPrice) continue;

                    int distance = CalculateDistance(fromLocationId, sellLocation.Id);
                    int travelCost = CalculateTravelCost(distance);
                    int netProfit = sellPrice - buyPrice - travelCost;

                    if (netProfit > 0)
                    {
                        opportunities.Add(new ArbitrageOpportunity
                        {
                            ItemId = item.Id,
                            ItemName = item.Name,
                            BuyLocationId = fromLocationId,
                            BuyLocationName = GetLocationName(fromLocationId),
                            BuyPrice = buyPrice,
                            SellLocationId = sellLocation.Id,
                            SellLocationName = sellLocation.Name,
                            SellPrice = sellPrice,
                            GrossProfit = sellPrice - buyPrice,
                            TravelCost = travelCost,
                            NetProfit = netProfit,
                            ProfitMargin = (float)netProfit / buyPrice,
                            RequiredCapital = buyPrice,
                            IsCurrentlyProfitable = true,
                            DistanceBetweenLocations = distance,
                            ProfitPerDistance = distance > 0 ? (float)netProfit / distance : netProfit
                        });
                    }
                }
            }

            return opportunities;
        }

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Calculate distance between two locations (simplified)
        /// </summary>
        private int CalculateDistance(string fromLocationId, string toLocationId)
        {
            // Simplified distance calculation
            // In a real implementation, this would use actual route data
            if (fromLocationId == toLocationId) return 0;

            // Define some basic distances between known locations
            Dictionary<string, Dictionary<string, int>> distances = new Dictionary<string, Dictionary<string, int>>
            {
                ["town_square"] = new Dictionary<string, int>
                {
                    ["dusty_flagon"] = 1,
                    ["old_mill"] = 2,
                    ["merchant_quarter"] = 1,
                    ["harbor"] = 3
                },
                ["dusty_flagon"] = new Dictionary<string, int>
                {
                    ["town_square"] = 1,
                    ["old_mill"] = 2,
                    ["merchant_quarter"] = 2,
                    ["harbor"] = 3
                },
                ["old_mill"] = new Dictionary<string, int>
                {
                    ["town_square"] = 2,
                    ["dusty_flagon"] = 2,
                    ["merchant_quarter"] = 3,
                    ["harbor"] = 4
                },
                ["merchant_quarter"] = new Dictionary<string, int>
                {
                    ["town_square"] = 1,
                    ["dusty_flagon"] = 2,
                    ["old_mill"] = 3,
                    ["harbor"] = 2
                },
                ["harbor"] = new Dictionary<string, int>
                {
                    ["town_square"] = 3,
                    ["dusty_flagon"] = 3,
                    ["old_mill"] = 4,
                    ["merchant_quarter"] = 2
                }
            };

            if (distances.ContainsKey(fromLocationId) &&
                distances[fromLocationId].ContainsKey(toLocationId))
            {
                return distances[fromLocationId][toLocationId];
            }

            // Default distance if not in our map
            return 2;
        }

        /// <summary>
        /// Calculate travel cost based on distance
        /// </summary>
        private int CalculateTravelCost(int distance)
        {
            // Simple linear cost model
            // Could be enhanced with route difficulty, transport costs, etc.
            return distance * 2; // 2 coins per unit distance
        }

        /// <summary>
        /// Get location name by ID
        /// </summary>
        private string GetLocationName(string locationId)
        {
            Location location = _gameWorld.WorldState.locations?.FirstOrDefault(l => l.Id == locationId);
            return location?.Name ?? locationId;
        }

        /// <summary>
        /// Generate human-readable opportunity description
        /// </summary>
        private string GenerateOpportunityDescription(string itemName, string buyLocation, string sellLocation, int profit)
        {
            if (profit > 20)
                return $"Excellent opportunity: Buy {itemName} in {buyLocation} and sell in {sellLocation} for {profit} coin profit!";
            else if (profit > 10)
                return $"Good trade: {itemName} from {buyLocation} to {sellLocation} yields {profit} coins";
            else
                return $"Modest profit: {itemName} trade between {buyLocation} and {sellLocation} for {profit} coins";
        }

        /// <summary>
        /// Generate route description
        /// </summary>
        private string GenerateRouteDescription(TradeRoute route)
        {
            if (route.Trades.Count == 0)
                return "No profitable trades found";

            List<string> trades = route.Trades.Select(t =>
                $"{t.ItemName} ({t.BuyLocationName} → {t.SellLocationName})"
            ).ToList();

            return $"Trade route with {route.TotalProfit} total profit: " + string.Join(" → ", trades);
        }
    }
}