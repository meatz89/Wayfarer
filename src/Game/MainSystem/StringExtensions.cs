public static class StringExtensions
{
    public static string SpaceBeforeCapitals(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        System.Text.StringBuilder result = new System.Text.StringBuilder(text.Length * 2);
        result.Append(text[0]);

        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                result.Append(' ');

            result.Append(text[i]);
        }

        return result.ToString();
    }
}