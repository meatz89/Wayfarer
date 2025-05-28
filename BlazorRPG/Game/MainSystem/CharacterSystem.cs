using System.Text;

public class CharacterSystem
{
    public string FormatKnownCharacters(List<NPC> characters)
    {
        StringBuilder sb = new StringBuilder();

        if (characters == null || !characters.Any())
            return "None";

        foreach (NPC character in characters)
        {
            sb.AppendLine($"- {character.Name}: {character.Role} at {character.Location}");
            if (!string.IsNullOrEmpty(character.Description))
                sb.AppendLine($"  Description: {character.Description}");
        }

        return sb.ToString();
    }
}
