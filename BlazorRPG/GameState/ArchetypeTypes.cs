
public enum ArchetypeTypes
{
    Guard, // Master of labor and physical work
    Rogue, // Thief and infiltrator
    Diplomat, // Master of salons, gossip, alliances, and noble networks
    Scholar, // Keeper of records and histories
    Merchant, // Hard-nosed trader and negotiator
    Explorer, // Potionbrewer and explorer
}

//| Class | Signature Skill(Excels) | Why ?                                                                                          |
//| --------------- | ------------------------ | ----------------------------------------------------------------------------------- |
//| **Guard**       | Endurance                | Laborer; master of sustained physical work                                          |
//| **Rogue**       | Finesse                  | Thief and infiltrator; master of stealth, nimble hands, and quiet steps             |
//| **Diplomat**    | Charm                    | Master of salons, gossip, alliances, and noble networks                             |
//| **Scholar**     | Lore                     | Keeper of records and histories; master of knowledge and careful study              |
//| **Merchant**    | Diplomacy                | Hard-nosed trader and negotiator; master of deals and persuasion                    |
//| **Explorer**    | Insight                  | explorer; master of observation, navigation, and situational awareness              |


public static class ArchetypeAffinities
{
    public static ArchetypeAffinity Guard = new ArchetypeAffinity { 
        ArchetypeType = ArchetypeTypes.Guard,
        NaturalAffinity = EncounterApproaches.Force, 
        IncompatibleAffinity = EncounterApproaches.Contemplation 
    };
    public static ArchetypeAffinity Rogue = new ArchetypeAffinity
    {
        ArchetypeType = ArchetypeTypes.Rogue,
        NaturalAffinity = EncounterApproaches.Precision,
        IncompatibleAffinity = EncounterApproaches.Rapport
    };

    public static ArchetypeAffinity Diplomat = new ArchetypeAffinity
    {
        ArchetypeType = ArchetypeTypes.Diplomat,
        NaturalAffinity = EncounterApproaches.Rapport,
        IncompatibleAffinity = EncounterApproaches.Precision
    };

    public static ArchetypeAffinity Merchant = new ArchetypeAffinity
    {
        ArchetypeType = ArchetypeTypes.Merchant,
        NaturalAffinity = EncounterApproaches.Persuasion,
        IncompatibleAffinity = EncounterApproaches.Observation
    };

    public static ArchetypeAffinity Explorer = new ArchetypeAffinity
    {
        ArchetypeType = ArchetypeTypes.Explorer,
        NaturalAffinity = EncounterApproaches.Observation,
        IncompatibleAffinity = EncounterApproaches.Persuasion
    };

    public static ArchetypeAffinity Scholar = new ArchetypeAffinity
    {
        ArchetypeType = ArchetypeTypes.Scholar,
        NaturalAffinity = EncounterApproaches.Contemplation,
        IncompatibleAffinity = EncounterApproaches.Force
    };

    public static EncounterApproaches GetNaturalForArchetype(ArchetypeTypes archetype)
    {
        switch (archetype)
        {
            case ArchetypeTypes.Guard:
                return Guard.NaturalAffinity;

            case ArchetypeTypes.Rogue:
                return Rogue.NaturalAffinity;

            case ArchetypeTypes.Diplomat:
                return Diplomat.NaturalAffinity;

            case ArchetypeTypes.Merchant:
                return Merchant.NaturalAffinity;

            case ArchetypeTypes.Explorer:
                return Explorer.NaturalAffinity;

            case ArchetypeTypes.Scholar:
                return Scholar.NaturalAffinity;
        }
        return EncounterApproaches.Neutral; // Default case
    }

    public static EncounterApproaches GetIncompatibleForArchetype(ArchetypeTypes archetype)
    {
        switch(archetype)
        {
            case ArchetypeTypes.Guard:
                return Guard.IncompatibleAffinity;

            case ArchetypeTypes.Rogue:
                return Rogue.IncompatibleAffinity;

            case ArchetypeTypes.Diplomat:
                return Diplomat.IncompatibleAffinity;

            case ArchetypeTypes.Merchant:
                return Merchant.IncompatibleAffinity;

            case ArchetypeTypes.Explorer:
                return Explorer.IncompatibleAffinity;

            case ArchetypeTypes.Scholar:
                return Scholar.IncompatibleAffinity;
        }
        return EncounterApproaches.Neutral; // Default case
    }
}

public class ArchetypeAffinity
{
    public ArchetypeTypes ArchetypeType;
    public EncounterApproaches NaturalAffinity;
    public EncounterApproaches IncompatibleAffinity;
}