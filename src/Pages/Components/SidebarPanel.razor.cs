using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace Wayfarer.Pages.Components;

public partial class SidebarPanel : ComponentBase
{
    [Inject] private GameFacade GameFacade { get; set; }

    private ElementReference SidebarRef;
    private Player PlayerState => GameFacade.GetPlayer();

    // Section expansion state
    private Dictionary<SidebarSections, bool> ExpandedSections = new Dictionary<SidebarSections, bool>
    {
        { SidebarSections.skills, false },
        { SidebarSections.resources, false },
        { SidebarSections.inventory, false }
    };

    private enum SidebarSections
    {
        skills,
        resources,
        inventory
    }

    private void ToggleSection(SidebarSections section)
    {
        ExpandedSections[section] = !ExpandedSections[section];
    }

    private string GetArchetypePortrait()
    {
        // Return appropriate portrait based on archetype
        return PlayerState?.Archetype switch
        {
            Professions.Soldier => "images/portraits/soldier.jpg",
            Professions.Merchant => "images/portraits/merchant.jpg",
            Professions.Scholar => "images/portraits/scholar.jpg",
            _ => "images/portraits/default.jpg"
        };
    }

    private List<(SkillTypes, string, string)> GetPlayerSkills()
    {
        return new List<(SkillTypes, string, string)>
        {
            (SkillTypes.BruteForce, "💪", "Brute Force"),
            (SkillTypes.Endurance, "🏃", "Endurance"),
            (SkillTypes.Acrobatics, "🤸", "Acrobatics"),
            (SkillTypes.Perception, "👁️", "Perception"),
            (SkillTypes.Knowledge, "🧠", "Knowledge"),
            (SkillTypes.Charm, "💬", "Charm")
        };
    }
}