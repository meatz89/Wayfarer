public class Player
{
    public int Coins { get; set; }
    public int Health { get; set; }
    public int MinHealth { get; set; }
    public int MaxHealth { get; set; }

    public int PhysicalEnergy { get; set; }
    public int MaxPhysicalEnergy { get; set; }

    public int FocusEnergy { get; set; }
    public int MaxFocusEnergy { get; set; }

    public int SocialEnergy { get; set; }
    public int MaxSocialEnergy { get; set; }

    public Dictionary<SkillTypes, int> Skills { get; set; }
    public Inventory Inventory { get; set; }
}
