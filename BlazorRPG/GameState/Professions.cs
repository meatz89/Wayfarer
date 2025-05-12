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
    public static ArchetypeAffinity Warrior = new ArchetypeAffinity { 
        ArchetypeType = Professions.Warrior,
        NaturalAffinity = EncounterApproaches.Force, 
        IncompatibleAffinity = EncounterApproaches.Contemplation 
    };
    public static ArchetypeAffinity Ranger = new ArchetypeAffinity
    {
        ArchetypeType = Professions.Ranger,
        NaturalAffinity = EncounterApproaches.Precision,
        IncompatibleAffinity = EncounterApproaches.Rapport
    };

    public static ArchetypeAffinity Diplomat = new ArchetypeAffinity
    {
        ArchetypeType = Professions.Diplomat,
        NaturalAffinity = EncounterApproaches.Rapport,
        IncompatibleAffinity = EncounterApproaches.Precision
    };

    public static ArchetypeAffinity Courtier = new ArchetypeAffinity
    {
        ArchetypeType = Professions.Courtier,
        NaturalAffinity = EncounterApproaches.Persuasion,
        IncompatibleAffinity = EncounterApproaches.Observation
    };

    public static ArchetypeAffinity Mystic = new ArchetypeAffinity
    {
        ArchetypeType = Professions.Mystic,
        NaturalAffinity = EncounterApproaches.Observation,
        IncompatibleAffinity = EncounterApproaches.Persuasion
    };

    public static ArchetypeAffinity Scholar = new ArchetypeAffinity
    {
        ArchetypeType = Professions.Scholar,
        NaturalAffinity = EncounterApproaches.Contemplation,
        IncompatibleAffinity = EncounterApproaches.Force
    };

    public static EncounterApproaches GetNaturalForArchetype(Professions archetype)
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
        return EncounterApproaches.Neutral; // Default case
    }

    public static EncounterApproaches GetIncompatibleForArchetype(Professions archetype)
    {
        switch(archetype)
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
        return EncounterApproaches.Neutral; // Default case
    }
}

public class ArchetypeAffinity
{
    public Professions ArchetypeType;
    public EncounterApproaches NaturalAffinity;
    public EncounterApproaches IncompatibleAffinity;
}