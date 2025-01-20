public class FileHelper
{
    public string ReadFile(string fileName)
    {
        string path = Path.Combine(GetAppDirectory(), fileName);

        string modelFromFile = string.Empty;
        using (FileStream stream = File.OpenRead(path))
        {
            StreamReader streamReader = new StreamReader(stream);
            try
            {
                modelFromFile = streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                string s = "";
            }
        }

        return modelFromFile;
    }

    private string GetAppDirectory()
    {
        return Environment.CurrentDirectory;
    }
}

