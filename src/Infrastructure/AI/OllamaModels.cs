public class OllamaRequest
{
public string Model { get; set; }

public string Prompt { get; set; }

public bool Stream { get; set; }
}

public class OllamaResponse
{
public string Response { get; set; }

public bool Done { get; set; }
}

public class OllamaStreamResponse
{
public string Response { get; set; }

public bool Done { get; set; }
}