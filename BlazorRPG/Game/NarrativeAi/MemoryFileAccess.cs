using Newtonsoft.Json;

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

            // Read all text from the file
            string memoryContentToWrite = string.Empty;
            if (File.Exists(filePath))
            {
                //memoryContentToWrite = await File.ReadAllTextAsync(filePath);
            }

            //memoryContentToWrite += Environment.NewLine;
            memoryContentToWrite += Environment.NewLine + memoryContent;

            // Write the memory content directly to the file
            await File.WriteAllTextAsync(filePath, memoryContentToWrite);
        }
        catch (Exception e)
        {
            // Log or handle the exception as needed
            Console.WriteLine($"Error writing to memory file: {e.Message}");
        }
    }

    public static async Task WriteToLogFile(WorldStateInput context)
    {
        string newContent = JsonConvert.SerializeObject(context);

        try
        {
            string _baseLogDirectory = Path.Combine("C:", "Logs");
            string filePath = Path.Combine(_baseLogDirectory, "WorldState");

            // Ensure directory exists
            Directory.CreateDirectory(_baseLogDirectory);

            // Read all text from the file
            string worldStateFileContent = string.Empty;
            if (File.Exists(filePath))
            {
                worldStateFileContent = await File.ReadAllTextAsync(filePath);
            }

            worldStateFileContent += Environment.NewLine;
            worldStateFileContent += Environment.NewLine + newContent;

            // Write the memory content directly to the file
            await File.WriteAllTextAsync(filePath, worldStateFileContent);
        }
        catch (Exception e)
        {
            // Log or handle the exception as needed
            Console.WriteLine($"Error writing to memory file: {e.Message}");
        }
    }
}