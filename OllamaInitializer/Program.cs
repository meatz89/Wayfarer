// OllamaInitializer/Program.cs
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OllamaInitializer
{
    public class Program
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string _ollamaBaseUrl = "http://ollama:11434";
        private static readonly string _modelName = "gemma3:12b-it-qat";
        private static readonly int _maxRetries = 10;
        private static readonly int _retryDelayMs = 5000;

        public static async Task Main(string[] args)
        {
            Console.WriteLine($"Starting Ollama model initialization for {_modelName}");

            // Wait for Ollama service to be ready
            await WaitForOllamaServiceAsync();

            // Check if model is already pulled
            if (await IsModelPulledAsync())
            {
                Console.WriteLine($"Model {_modelName} is already pulled");
                return;
            }

            // Pull the model
            await PullModelAsync();

            Console.WriteLine("Initialization complete");
        }

        private static async Task WaitForOllamaServiceAsync()
        {
            for (int i = 0; i < _maxRetries; i++)
            {
                try
                {
                    Console.WriteLine($"Attempt {i + 1}/{_maxRetries} to connect to Ollama service...");
                    HttpResponseMessage response = await _httpClient.GetAsync($"{_ollamaBaseUrl}/api/tags");

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Ollama service is ready");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ollama service not ready: {ex.Message}");
                }

                await Task.Delay(_retryDelayMs);
            }

            throw new Exception("Failed to connect to Ollama service after maximum retry attempts");
        }

        private static async Task<bool> IsModelPulledAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_ollamaBaseUrl}/api/tags");
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    JsonDocument doc = JsonDocument.Parse(responseContent);

                    if (doc.RootElement.TryGetProperty("models", out JsonElement modelsElement))
                    {
                        foreach (JsonElement model in modelsElement.EnumerateArray())
                        {
                            if (model.TryGetProperty("name", out JsonElement nameElement) &&
                                nameElement.GetString() == _modelName)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if model is pulled: {ex.Message}");
            }

            return false;
        }

        private static async Task PullModelAsync()
        {
            try
            {
                Console.WriteLine($"Pulling model {_modelName}...");

                object requestBody = new { name = _modelName };
                string jsonRequest = JsonSerializer.Serialize(requestBody);

                HttpResponseMessage response = await _httpClient.PostAsync(
                    $"{_ollamaBaseUrl}/api/pull",
                    new StringContent(jsonRequest, Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Successfully pulled model {_modelName}");
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to pull model: {response.StatusCode}\n{errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error pulling model: {ex.Message}");
                throw;
            }
        }
    }
}