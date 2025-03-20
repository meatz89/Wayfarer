using System.Text.Json.Serialization;

public class EncounterOutcome
    {
        [JsonPropertyName("outcome_level")]
        public string OutcomeLevel { get; set; } = "";

        [JsonPropertyName("goal_achieved")]
        public bool GoalAchieved { get; set; }

        [JsonPropertyName("success_details")]
        public string SuccessDetails { get; set; } = "";
    }

    public class InventoryItem
    {
        [JsonPropertyName("item")]
        public string Item { get; set; } = "";

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("acquired")]
        public bool Acquired { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
    }

    public class ResourceChange
    {
        [JsonPropertyName("money_amount")]
        public int MoneyAmount { get; set; }

        [JsonPropertyName("money_currency")]
        public string MoneyCurrency { get; set; } = "";

        [JsonPropertyName("money_details")]
        public string MoneyDetails { get; set; } = "";

        [JsonPropertyName("food_quantity")]
        public int FoodQuantity { get; set; }

        [JsonPropertyName("food_meals_worth")]
        public int FoodMealsWorth { get; set; }

        [JsonPropertyName("food_description")]
        public string FoodDescription { get; set; } = "";

        [JsonPropertyName("food_duration")]
        public string FoodDuration { get; set; } = "";

        [JsonPropertyName("health_change")]
        public int HealthChange { get; set; }

        [JsonPropertyName("health_details")]
        public string HealthDetails { get; set; } = "";
    }

    public class DiscoveredLocation
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("npcs_present")]
        public List<string> NPCsPresent { get; set; } = new List<string>();

        [JsonPropertyName("opportunities")]
        public List<string> Opportunities { get; set; } = new List<string>();
    }

    public class NPC
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("role")]
        public string Role { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("location")]
        public string Location { get; set; } = "";

        [JsonPropertyName("relationship")]
        public string Relationship { get; set; } = "";

        [JsonPropertyName("disposition")]
        public string Disposition { get; set; } = "";
    }

    public class RelationshipChange
    {
        [JsonPropertyName("npc")]
        public string NPC { get; set; } = "";

        [JsonPropertyName("change")]
        public string Change { get; set; } = "";

        [JsonPropertyName("details")]
        public string Details { get; set; } = "";

        [JsonPropertyName("current_state")]
        public string CurrentState { get; set; } = "";
    }

    public class Quest2
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("giver")]
        public string Giver { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("location")]
        public string Location { get; set; } = "";

        [JsonPropertyName("reward")]
        public string Reward { get; set; } = "";

        [JsonPropertyName("urgency")]
        public string Urgency { get; set; } = "";

        [JsonPropertyName("expires")]
        public bool Expires { get; set; }

        [JsonPropertyName("expiration_condition")]
        public string ExpirationCondition { get; set; } = "";
    }

    public class Job
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("employer")]
        public string Employer { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("location")]
        public string Location { get; set; } = "";

        [JsonPropertyName("payment")]
        public string Payment { get; set; } = "";

        [JsonPropertyName("duration")]
        public string Duration { get; set; } = "";

        [JsonPropertyName("start_time")]
        public string StartTime { get; set; } = "";

        [JsonPropertyName("recurring")]
        public bool Recurring { get; set; }
    }

    public class Rumor
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = "";

        [JsonPropertyName("source")]
        public string Source { get; set; } = "";

        [JsonPropertyName("reliability")]
        public string Reliability { get; set; } = "";

        [JsonPropertyName("related_to")]
        public string RelatedTo { get; set; } = "";
    }

    public class TimePassage
    {
        [JsonPropertyName("time_elapsed")]
        public string TimeElapsed { get; set; } = "";

        [JsonPropertyName("current_time")]
        public string CurrentTime { get; set; } = "";

        [JsonPropertyName("current_day")]
        public string CurrentDay { get; set; } = "";
}
