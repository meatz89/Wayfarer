public class MemoryFileAccess
{
    const string fileName = "memory.txt";

    public static async Task<string> ReadFromMemoryFile()
    {
        string memoryContent = string.Empty;
        try
        {
            string _baseLogDirectory = Path.Combine("C:", "Logs");
            string filePath = Path.Combine(_baseLogDirectory, fileName);

            // Ensure directory exists
            Directory.CreateDirectory(_baseLogDirectory);

            // Read all text from the file
            memoryContent = await File.ReadAllTextAsync(filePath);
        }
        catch (Exception e)
        {
            // Log or handle the exception as needed
            Console.WriteLine($"Error reading memory file: {e.Message}");
        }
        return memoryContent;
    }

    public static async Task WriteToMemoryFile(string memoryContent)
    {
        try
        {
            string _baseLogDirectory = Path.Combine("C:", "Logs");
            string filePath = Path.Combine(_baseLogDirectory, fileName);

            // Ensure directory exists
            Directory.CreateDirectory(_baseLogDirectory);

            // Write the memory content directly to the file
            await File.WriteAllTextAsync(filePath, memoryContent);
        }
        catch (Exception e)
        {
            // Log or handle the exception as needed
            Console.WriteLine($"Error writing to memory file: {e.Message}");
        }
    }
}