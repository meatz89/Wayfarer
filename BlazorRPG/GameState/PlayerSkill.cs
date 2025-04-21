public class PlayerSkills
{
    private static Dictionary<SkillTypes, int> List = new Dictionary<SkillTypes, int>();

    public static int Subterfuge => List[SkillTypes.Subterfuge];
    public static int Warfare => List[SkillTypes.Warfare];
    public static int Wilderness => List[SkillTypes.Wilderness];
    public static int Scholarship => List[SkillTypes.Scholarship];
    public static int Diplomacy => List[SkillTypes.Diplomacy];

    public static void Initialize()
    {
        List.Add(SkillTypes.Subterfuge, 1);
        List.Add(SkillTypes.Warfare, 1);
        List.Add(SkillTypes.Wilderness, 1);
        List.Add(SkillTypes.Scholarship, 1);
        List.Add(SkillTypes.Diplomacy, 1);
    }

    public PlayerSkills()
    {
        Initialize();
    }

    public void ImproveSkill(SkillTypes skillType, int level)
    {
        int current = List[skillType];
        int newlevel = current + level;
        List[skillType] = newlevel;
    }

    public int GetLevelForSkill(SkillTypes skillType)
    {
        return List[skillType];
    }
}