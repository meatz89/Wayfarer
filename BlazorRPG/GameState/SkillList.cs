// Enum for storing all possible approach-encounter type combinations
public class SkillList
{
    private List<Skill> _skills = new List<Skill>();

    public int GetSkillLevel(string skillName)
    {
        foreach (Skill skill in _skills)
        {
            if (skill.Name == skillName)
                return skill.Level;
        }
        return 0;
    }

    public void SetSkillLevel(string skillName, int level)
    {
        foreach (Skill skill in _skills)
        {
            if (skill.Name == skillName)
            {
                skill.Level = level;
                return;
            }
        }

        // Add new skill if not found
        _skills.Add(new Skill { Name = skillName, Level = level });
    }

    public List<Skill> GetAllSkills()
    {
        return new List<Skill>(_skills);
    }
}
