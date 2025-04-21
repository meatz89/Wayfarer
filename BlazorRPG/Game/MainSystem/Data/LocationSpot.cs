public class LocationSpot
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string InteractionType { get; set; }
    public List<string> ActionIds { get; set; } = new();

    public string LocationName { get; set; }
    public List<string> ResidentCharacterIds { get; set; } = new List<string>();
    public List<string> AssociatedOpportunityIds { get; set; } = new List<string>();

    public string InteractionDescription { get; set; }

    public Population? Population { get; set; } = Population.Quiet;
    public Atmosphere? Atmosphere { get; set; } = Atmosphere.Calm;
    public Physical? Physical { get; set; } = Physical.Confined;
    public Illumination? Illumination { get; set; } = Illumination.Bright;
    public bool PlayerKnowledge { get; set; }
    public string Character { get; set; }

    public ResourceNodeTypes NodeType { get; set; } = ResourceNodeTypes.None;
    public QualityTiers Quality { get; set; } = QualityTiers.Novice;
    public float BaseYield { get; internal set; } = 1;
    public float CurrentDepletion { get; set; } = 0f;
    public float BaseDepletion { get; set; } = 0.2f;
    public float ReplenishRate { get; set; } = 0.2f; // Per game hour
    public List<NodeAspectDefinition> DiscoverableAspects { get; set; } = new List<NodeAspectDefinition>();

    // Update based on time
    public void OnTimeChanged(TimeWindows newTimeWindow)
    {
        // Replenish resources based on time passed
        ReplenishResources(1); // Assuming 1 hour has passed

        // Update environmental properties based on time
        UpdatePropertiesForTime(newTimeWindow);
    }

    public void ReplenishResources(int hours)
    {
        float replenishAmount = ReplenishRate * hours;
        CurrentDepletion = Math.Max(0.0f, CurrentDepletion - replenishAmount);
    }

    public void Deplete(float amount)
    {
        CurrentDepletion = Math.Min(1.0f, CurrentDepletion + amount);
    }

    private void UpdatePropertiesForTime(TimeWindows timeWindow)
    {
        // Each node type can have specific time-based adjustments
        switch (NodeType)
        {
            case ResourceNodeTypes.Food:
                // Food spots might be more plentiful in morning
                if (timeWindow == TimeWindows.Morning)
                    ReplenishRate = 0.3f;
                else
                    ReplenishRate = 0.2f;
                break;

            case ResourceNodeTypes.Water:
                // Water clearer during day
                break;

                // Other node types...
        }
    }
}
