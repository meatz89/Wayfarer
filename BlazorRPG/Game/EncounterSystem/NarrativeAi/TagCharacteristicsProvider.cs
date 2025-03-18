
// Provides constants for tag characteristics
public static class TagCharacteristicsProvider
{
    public static string GetApproachCharacteristics(string approach)
    {
        switch (approach.ToLower())
        {
            case "force":
                return "Direct, assertive, physical, strong";
            case "finesse":
                return "Careful, precise, skilled, attentive";
            case "wit":
                return "Clever, observant, strategic, analytical";
            case "charm":
                return "Friendly, appealing, warm, personable";
            case "stealth":
                return "Subtle, quiet, unobtrusive, indirect";
            default:
                return approach;
        }
    }

    public static string GetFocusCharacteristics(string focus)
    {
        switch (focus.ToLower())
        {
            case "relationship":
                return "Connection with people, status, trust";
            case "information":
                return "Facts, knowledge, secrets, details";
            case "physical":
                return "Bodies, items, direct interaction";
            case "environment":
                return "Surroundings, location features, conditions";
            case "resource":
                return "Money, supplies, time, valuables";
            default:
                return focus;
        }
    }
}
