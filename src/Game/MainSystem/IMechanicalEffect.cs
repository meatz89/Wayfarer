using System.Collections.Generic;

public interface IMechanicalEffect
{
    void Apply(ConversationState state);
    List<MechanicalEffectDescription> GetDescriptionsForPlayer();
}

public class NoEffect : IMechanicalEffect
{
    public void Apply(ConversationState state) { }
    public List<MechanicalEffectDescription> GetDescriptionsForPlayer() 
    { 
        return new List<MechanicalEffectDescription> 
        { 
            new MechanicalEffectDescription 
            { 
                Text = "No effect", 
                Category = EffectCategory.StateChange 
            }
        }; 
    }
}

public class AdvanceDurationEffect : IMechanicalEffect
{
    private int amount;

    public AdvanceDurationEffect(int amount)
    {
        this.amount = amount;
    }

    public void Apply(ConversationState state)
    {
        state.DurationCounter += amount;
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> 
        { 
            new MechanicalEffectDescription 
            { 
                Text = "Time advances",
                Category = EffectCategory.TimePassage,
                TimeMinutes = amount * 5
            }
        };
    }
}

public class CreateMemoryEffect : IMechanicalEffect
{
    private string memoryKey;
    private string memoryDescription;
    private int importance;
    private int expirationDays;

    public CreateMemoryEffect(string memoryKey, string memoryDescription, int importance, int expirationDays = -1)
    {
        this.memoryKey = memoryKey;
        this.memoryDescription = memoryDescription;
        this.importance = importance;
        this.expirationDays = expirationDays;
    }

    public void Apply(ConversationState state)
    {
        state.Player.AddMemory(memoryKey, memoryDescription, importance, expirationDays);
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> 
        { 
            new MechanicalEffectDescription 
            { 
                Text = "You will remember this event",
                Category = EffectCategory.InformationGain
            }
        };
    }
}

// Create a memory check effect class
public class CheckMemoryEffect : IMechanicalEffect
{
    private string memoryKey;
    private IMechanicalEffect effectIfPresent;
    private IMechanicalEffect effectIfAbsent;

    public CheckMemoryEffect(string memoryKey, IMechanicalEffect effectIfPresent, IMechanicalEffect effectIfAbsent)
    {
        this.memoryKey = memoryKey;
        this.effectIfPresent = effectIfPresent;
        this.effectIfAbsent = effectIfAbsent;
    }

    public void Apply(ConversationState state)
    {
        if (state.Player.HasMemory(memoryKey, state.GameWorld?.CurrentDay ?? 1))
        {
            effectIfPresent.Apply(state);
        }
        else
        {
            effectIfAbsent.Apply(state);
        }
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> 
        { 
            new MechanicalEffectDescription 
            { 
                Text = "Your past experiences affect this outcome",
                Category = EffectCategory.StateChange
            }
        };
    }

    public class CompleteGoalRequirementEffect : IMechanicalEffect
    {
        private string goalName;
        private string requirementName;

        public CompleteGoalRequirementEffect(string goalName, string requirementName)
        {
            this.goalName = goalName;
            this.requirementName = requirementName;
        }

        public void Apply(ConversationState state)
        {
            Goal goal = state.Player.ActiveGoals.FirstOrDefault(g => g.Name == goalName);
            if (goal != null)
            {
                goal.CompleteRequirement(requirementName);
            }
        }

        public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
        {
            return new List<MechanicalEffectDescription>
            {
                new MechanicalEffectDescription
                {
                    Text = $"Progress toward {goalName}",
                    Category = EffectCategory.StateChange
                }
            };
        }
    }

    public class AddGoalEffect : IMechanicalEffect
    {
        private Goal goalToAdd;

        public AddGoalEffect(Goal goalToAdd)
        {
            this.goalToAdd = goalToAdd;
        }

        public void Apply(ConversationState state)
        {
            state.Player.AddGoal(goalToAdd);
        }

        public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
        {
            return new List<MechanicalEffectDescription>
            {
                new MechanicalEffectDescription
                {
                    Text = $"New goal: {goalToAdd.Name}",
                    Category = EffectCategory.StateChange
                }
            };
        }
    }

    public class DiscoverRouteEffect : IMechanicalEffect
    {
        private RouteOption routeToDiscover;

        public DiscoverRouteEffect(RouteOption routeToDiscover)
        {
            this.routeToDiscover = routeToDiscover;
        }

        public void Apply(ConversationState state)
        {
            state.Player.AddKnownRoute(routeToDiscover);
        }

        public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
        {
            return new List<MechanicalEffectDescription>
            {
                new MechanicalEffectDescription
                {
                    Text = $"Discovered route to {routeToDiscover.Destination}",
                    Category = EffectCategory.RouteUnlock,
                    RouteName = routeToDiscover.Name
                }
            };
        }
    }


    // Flag system removed - using connection tokens instead

    public class ModifyFocusEffect : IMechanicalEffect
    {
        private int amount;

        public ModifyFocusEffect(int amount)
        {
            this.amount = amount;
        }

        public void Apply(ConversationState state)
        {
            state.FocusPoints += amount;
            state.FocusPoints = Math.Max(0, Math.Min(state.FocusPoints, state.MaxFocusPoints));
        }

        public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
        {
            return new List<MechanicalEffectDescription>
            {
                new MechanicalEffectDescription
                {
                    Text = amount > 0 ? $"Regain {amount} Focus" : $"Lose {Math.Abs(amount)} Focus",
                    Category = EffectCategory.StateChange
                }
            };
        }
    }

    // Flag system removed - using connection tokens instead

    public class FocusChangeEffect : IMechanicalEffect
    {
        private int amount;

        public FocusChangeEffect(int amount)
        {
            this.amount = amount;
        }

        public void Apply(ConversationState state)
        {
            if (amount > 0)
            {
                // Cannot exceed maximum
                state.FocusPoints = Math.Min(state.FocusPoints + amount, state.MaxFocusPoints);
            }
            else
            {
                // Cannot go below zero
                state.FocusPoints = Math.Max(0, state.FocusPoints + amount);
            }
        }

        public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
        {
            return new List<MechanicalEffectDescription>
            {
                new MechanicalEffectDescription
                {
                    Text = amount > 0 ? $"+{amount} Focus Point{(amount > 1 ? "s" : "")}" : $"{amount} Focus Point{(amount < -1 ? "s" : "")}",
                    Category = EffectCategory.StateChange
                }
            };
        }
    }

    public class DurationAdvanceEffect : IMechanicalEffect
    {
        private int amount;

        public DurationAdvanceEffect(int amount)
        {
            this.amount = amount;
        }

        public void Apply(ConversationState state)
        {
            state.AdvanceDuration(amount);
        }

        public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
        {
            return new List<MechanicalEffectDescription>
            {
                new MechanicalEffectDescription
                {
                    Text = $"Advances conversation duration by {amount}",
                    Category = EffectCategory.TimePassage,
                    TimeMinutes = amount * 5
                }
            };
        }
    }

    public class ProgressChangeEffect : IMechanicalEffect
    {
        private int amount;

        public ProgressChangeEffect(int amount)
        {
            this.amount = amount;
        }

        public void Apply(ConversationState state)
        {
            // Progress tracking removed - conversations use duration
        }

        public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
        {
            return new List<MechanicalEffectDescription>
            {
                new MechanicalEffectDescription
                {
                    Text = amount > 0 ? $"+{amount} Progress" : $"{amount} Progress",
                    Category = EffectCategory.StateChange
                }
            };
        }
    }

    public class CompoundEffect : IMechanicalEffect
    {
        private List<IMechanicalEffect> effects;

        public CompoundEffect(List<IMechanicalEffect> effects)
        {
            this.effects = effects;
        }

        public void Apply(ConversationState state)
        {
            foreach (IMechanicalEffect effect in effects)
            {
                effect.Apply(state);
            }
        }

        public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
        {
            var descriptions = new List<MechanicalEffectDescription>();
            
            foreach (IMechanicalEffect effect in effects)
            {
                descriptions.AddRange(effect.GetDescriptionsForPlayer());
            }
            
            return descriptions;
        }
    }
}