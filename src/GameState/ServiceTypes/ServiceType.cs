namespace Wayfarer.GameState;

public abstract class ServiceType
{
    public abstract string Id { get; }

    public abstract string DisplayName { get; }

    public abstract ChoiceReward GenerateRewards(int tier);

    public virtual string GetNegotiationContext()
    {
        return $"{Id}_negotiation";
    }

    public abstract SceneArchetypeDefinition GenerateMultiSituationArc(
        int tier,
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer);

    public abstract SceneArchetypeDefinition GenerateSingleSituation(
        int tier,
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer);

    public virtual NarrativeHints GenerateNegotiationHints(
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer)
    {
        NarrativeHints hints = new NarrativeHints();

        if (contextNPC.PersonalityType == PersonalityType.DEVOTED)
        {
            hints.Tone = "empathetic";
            hints.Theme = "human_connection";
        }
        else if (contextNPC.PersonalityType == PersonalityType.MERCANTILE)
        {
            hints.Tone = "transactional";
            hints.Theme = "economic_exchange";
        }
        else
        {
            hints.Tone = "professional";
            hints.Theme = "service_request";
        }

        hints.Context = GetNegotiationContext();

        if (contextPlayer.Coins < 10)
        {
            hints.Style = "desperate";
        }
        else
        {
            hints.Style = "standard";
        }

        return hints;
    }
}
