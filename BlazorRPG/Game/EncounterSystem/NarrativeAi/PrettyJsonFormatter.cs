using Serilog.Events;
using Serilog.Formatting;
using System.Text.Json;

public class PrettyJsonFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        // Get the rendered message (should be a JSON string)
        string message = logEvent.RenderMessage();
        try
        {
            using (JsonDocument doc = JsonDocument.Parse(message))
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                // Serialize the JSON element with indentation
                string prettyJson = JsonSerializer.Serialize(doc.RootElement, options);
                output.Write(prettyJson);
            }
        }
        catch (Exception)
        {
            // Fallback: output the original message if parsing fails
            output.Write(message);
        }
    }
}

