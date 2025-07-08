using System.Text;

public class ConsoleResponseWatcher : IResponseStreamWatcher
{
    private StringBuilder _responseBuilder = new StringBuilder();
    private int _currentLineLength = 0;
    private int _totalLength = 0;
    private int _maxLineLength;

    public ConsoleResponseWatcher()
    {
        // Get console width and leave some margin
        _maxLineLength = Console.WindowWidth - 5;
        if (_maxLineLength < 20) _maxLineLength = 80; // Fallback if console width is unavailable

        // Show initial prompt
        Console.Write("Response: ");
        _currentLineLength = 10; // "Response: " length
    }

    public void OnStreamUpdate(string chunk)
    {
        // Append to our complete response
        _responseBuilder.Append(chunk);
        _totalLength += chunk.Length;

        // Process the chunk character by character for better line wrapping
        foreach (char c in chunk)
        {
            // Handle newlines in the response
            if (c == '\n' || c == '\r')
            {
                if (c == '\n')
                {
                    Console.WriteLine();
                    _currentLineLength = 0;
                }
                continue;
            }

            // Check if we need a new line
            if (_currentLineLength >= _maxLineLength)
            {
                Console.WriteLine();
                _currentLineLength = 0;
            }

            // Write the character and increment line length
            Console.Write(c);
            _currentLineLength++;
        }
    }

    public void OnStreamComplete(string completeResponse)
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
