using System.Text;

public class CharacterSystem
{
    public string FormatKnownCharacters(List<Character> characters)
    {
        StringBuilder sb = new StringBuilder();

        if (characters == null || !characters.Any())
            return "None";

        foreach (Character character in characters)
        {
            sb.AppendLine($"- {character.Name}: {character.Role} at {character.Location}");
            if (!string.IsNullOrEmpty(character.Description))
                sb.AppendLine($"  Description: {character.Description}");
        }

        return sb.ToString();
    }
}
