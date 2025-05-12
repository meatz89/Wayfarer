using System.Text;

public class ConsoleResponseWatcher : IResponseStreamWatcher
{
    private readonly StringBuilder _responseBuilder = new StringBuilder();
    private int _totalLength = 0;

    public void OnResponseChunk(string chunk)
    {
        // Append to our complete response
        _responseBuilder.Append(chunk);
        _totalLength += chunk.Length;

        // Clear the current line and show the growing response
        Console.Write("\r" + new string(' ', Console.WindowWidth - 1)); // Clear line
        Console.Write("\rResponse: ");

        // Get the full response so far, but limit display length based on console width
        string displayText = _responseBuilder.ToString()
            .Replace("\r", "")
            .Replace("\n", " "); // Replace newlines with spaces

        // Calculate max chars we can show (account for "Response: " prefix)
        int maxLength = Console.WindowWidth - 12;
        if (maxLength < 10) maxLength = 10; // Minimum display width

        // If response is too long, show the tail portion with ellipsis
        if (displayText.Length > maxLength)
        {
            displayText = "..." + displayText.Substring(displayText.Length - maxLength + 3);
        }

        Console.Write(displayText);
    }

    public void OnResponseComplete(string completeResponse)
    {
        // Final line break and completion message
        Console.WriteLine();
        Console.WriteLine($"\nResponse complete. Total length: {_totalLength} characters");
    }

    public void OnError(Exception ex)
    {
        Console.WriteLine($"\n[ERROR] {ex.Message}");
    }
}