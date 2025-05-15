public enum Professions
{
    Warrior,
    Scholar,
    Courtier,
    Ranger,
    Mystic,
    Diplomat
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
    public static ArchetypeAffinity Warrior = new ArchetypeAffinity
    {
        ArchetypeType = Professions.Warrior,
        NaturalAffinity = EncounterCategories.Force,
        IncompatibleAffinity = EncounterCategories.Contemplation
    };
    public static ArchetypeAffinity Ranger = new ArchetypeAffinity
    {
        ArchetypeType = Professions.Ranger,
        NaturalAffinity = EncounterCategories.Precision,
        IncompatibleAffinity = EncounterCategories.Rapport
    };

    public static ArchetypeAffinity Diplomat = new ArchetypeAffinity
    {
        ArchetypeType = Professions.Diplomat,
        NaturalAffinity = EncounterCategories.Rapport,
        IncompatibleAffinity = EncounterCategories.Precision
    };

    public static ArchetypeAffinity Courtier = new ArchetypeAffinity
    {
        ArchetypeType = Professions.Courtier,
        NaturalAffinity = EncounterCategories.Persuasion,
        IncompatibleAffinity = EncounterCategories.Observation
    };

    public static ArchetypeAffinity Mystic = new ArchetypeAffinity
    {
        ArchetypeType = Professions.Mystic,
        NaturalAffinity = EncounterCategories.Observation,
        IncompatibleAffinity = EncounterCategories.Persuasion
    };

    public static ArchetypeAffinity Scholar = new ArchetypeAffinity
    {
        ArchetypeType = Professions.Scholar,
        NaturalAffinity = EncounterCategories.Contemplation,
        IncompatibleAffinity = EncounterCategories.Force
    };

    public static EncounterCategories GetNaturalForArchetype(Professions archetype)
    {
        switch (archetype)
        {
            case Professions.Warrior:
                return Warrior.NaturalAffinity;

            case Professions.Ranger:
                return Ranger.NaturalAffinity;

            case Professions.Diplomat:
                return Diplomat.NaturalAffinity;

            case Professions.Courtier:
                return Courtier.NaturalAffinity;

            case Professions.Mystic:
                return Mystic.NaturalAffinity;

            case Professions.Scholar:
                return Scholar.NaturalAffinity;
        }
        return EncounterCategories.None; // Default case
    }

    public static EncounterCategories GetIncompatibleForArchetype(Professions archetype)
    {
        switch (archetype)
        {
            case Professions.Warrior:
                return Warrior.IncompatibleAffinity;

            case Professions.Ranger:
                return Ranger.IncompatibleAffinity;

            case Professions.Diplomat:
                return Diplomat.IncompatibleAffinity;

            case Professions.Courtier:
                return Courtier.IncompatibleAffinity;

            case Professions.Mystic:
                return Mystic.IncompatibleAffinity;

            case Professions.Scholar:
                return Scholar.IncompatibleAffinity;
        }
        return EncounterCategories.None; // Default case
    }
}

public class ArchetypeAffinity
{
    public Professions ArchetypeType;
    public EncounterCategories NaturalAffinity;
    public EncounterCategories IncompatibleAffinity;
}