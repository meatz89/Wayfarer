public class EncounterChoiceSlot
{
    public BasicActionTypes BasicActionTypeCondition;
    public EncounterStateCondition EncounterStateCondition;
    public EncounterChoiceTemplate ChoiceTemplate;

    public bool IsValidFor(EncounterContext context)
    {
        if(context.ActionType != BasicActionTypeCondition) return false;
        if(!MeetsEncounterStateConditions(context.CurrentValues)) return false;
        return true;
    }

    private bool MeetsEncounterStateConditions(EncounterValues values)
    {
        if(!EncounterStateCondition.IsMet(values)) return false;
        return true;
    }
}
