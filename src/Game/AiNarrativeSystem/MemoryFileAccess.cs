using Newtonsoft.Json;

public class MemoryFileAccess
{
    const string fileName = "memory.txt";
    const string fileNameWorld = "worldState.txt";

    // Static SemaphoreSlim instances to control file access
    private SemaphoreSlim memorySemaphore = new SemaphoreSlim(1, 1);
    private SemaphoreSlim worldStateSemaphore = new SemaphoreSlim(1, 1);
    private Guid gameInstanceId;

    public MemoryFileAccess(Guid guid)
    {
        this.gameInstanceId = guid;
    }

    private async Task<List<string>> ReadFromMemoryFile()
    {
        List<string> memoryContent = new();
        string _baseLogDirectory = Path.Combine("C:", "Logs");
        string memoryFileName = this.gameInstanceId.ToString() + "_" + fileName;
        string filePath = Path.Combine(_baseLogDirectory, memoryFileName);

        // Ensure directory exists
        Directory.CreateDirectory(_baseLogDirectory);

        // Wait to acquire the semaphore
        await memorySemaphore.WaitAsync();
        try
        {
            // Read all text from the file
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error reading memory file");
                return memoryContent; // Return empty list if file does not exist
            }
            string[] strings = await File.ReadAllLinesAsync(filePath);
            memoryContent = strings.ToList();
            return memoryContent;
        }
        finally
        {
            // Always release the semaphore when done
            memorySemaphore.Release();
        }
    }

    public async Task WriteToMemoryFile(string memoryContent)
    {
        string _baseLogDirectory = Path.Combine("C:", "Logs");
        string filePath = Path.Combine(_baseLogDirectory, fileName);

        // Ensure directory exists
        Directory.CreateDirectory(_baseLogDirectory);

        // Wait to acquire the semaphore
        await memorySemaphore.WaitAsync();
        try
        {
            // Format the memory content
            string memoryContentToWrite = Environment.NewLine + memoryContent;

            // Write the memory content directly to the file with FileShare.None
            using (FileStream fileStream = new FileStream(
                filePath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.None))
            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                await writer.WriteLineAsync(memoryContentToWrite);
            }
        }
        catch (Exception e)
        {
            // Log or handle the exception as needed
            Console.WriteLine($"Error writing to memory file: {e.Message}");
        }
        finally
        {
            // Always release the semaphore when done
            memorySemaphore.Release();
        }
    }

    public async Task WriteToLogFile(WorldStateInput context)
    {
        string newContent = JsonConvert.SerializeObject(context);
        string _baseLogDirectory = Path.Combine("C:", "Logs");
        string filePath = Path.Combine(_baseLogDirectory, fileNameWorld);

        // Ensure directory exists
        Directory.CreateDirectory(_baseLogDirectory);

        // Wait to acquire the semaphore
        await worldStateSemaphore.WaitAsync();
        try
        {
            // Ensure the file exists before opening with FileMode.Truncate
            if (!File.Exists(filePath))
            {
                using (FileStream fs = File.Create(filePath))
                {
                    // Optionally write an empty string or leave as is
                }
            }

            // Format the content for appending
            string contentToAppend = Environment.NewLine + Environment.NewLine + newContent;

            // Write the memory content directly to the file with FileShare.None
            using (FileStream fileStream = new FileStream(
                filePath,
                FileMode.Truncate,
                FileAccess.Write,
                FileShare.None))
            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                await writer.WriteAsync(contentToAppend);
            }
        }
        catch (Exception e)
        {
            // Log or handle the exception as needed
            Console.WriteLine($"Error writing to world state file: {e.Message}");
        }
        finally
        {
            // Always release the semaphore when done
            worldStateSemaphore.Release();
        }
    }

    public async Task<List<string>> GetAllMemories()
    {
        return await ReadFromMemoryFile();
    }
}