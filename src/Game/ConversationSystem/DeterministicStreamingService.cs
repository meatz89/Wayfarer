using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Service that simulates text streaming for deterministic narrative content.
/// Ensures consistent UI experience between AI and deterministic providers.
/// </summary>
public class DeterministicStreamingService
{
    private readonly IConfiguration _configuration;

    // Configuration defaults
    private const int DEFAULT_DELAY_PER_WORD_MS = 50;
    private const int DEFAULT_DELAY_PER_CHARACTER_MS = 10;
    private const bool DEFAULT_USE_WORD_MODE = true;

    public DeterministicStreamingService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Streams text through the provided watchers, simulating typing effect
    /// </summary>
    public async Task StreamTextAsync(string text, List<IResponseStreamWatcher> watchers)
    {
        if (string.IsNullOrEmpty(text))
        {
            foreach (IResponseStreamWatcher watcher in watchers)
            {
                watcher.OnStreamComplete(string.Empty);
            }
            return;
        }

        // Get configuration
        bool useWordMode = _configuration.GetValue<bool?>("DeterministicStreaming:UseWordMode") ?? DEFAULT_USE_WORD_MODE;
        int delayMs = useWordMode
            ? _configuration.GetValue<int?>("DeterministicStreaming:DelayPerWordMs") ?? DEFAULT_DELAY_PER_WORD_MS
            : _configuration.GetValue<int?>("DeterministicStreaming:DelayPerCharacterMs") ?? DEFAULT_DELAY_PER_CHARACTER_MS;

        // Begin streaming for all watchers
        foreach (IResponseStreamWatcher watcher in watchers)
        {
            if (watcher is StreamingContentStateWatcher streamWatcher)
            {
                streamWatcher.BeginStreaming();
            }
        }

        try
        {
            if (useWordMode)
            {
                await StreamByWords(text, watchers, delayMs);
            }
            else
            {
                await StreamByCharacters(text, watchers, delayMs);
            }
        }
        catch (Exception ex)
        {
            // Notify watchers of error
            foreach (IResponseStreamWatcher watcher in watchers)
            {
                watcher.OnError(ex);
            }
            throw;
        }
    }

    private async Task StreamByWords(string text, List<IResponseStreamWatcher> watchers, int delayMs)
    {
        string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        StringBuilder buffer = new StringBuilder();

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            string chunk = i == 0 ? word : " " + word;
            buffer.Append(chunk);

            // Update all watchers with this chunk
            foreach (IResponseStreamWatcher watcher in watchers)
            {
                watcher.OnStreamUpdate(chunk);
            }

            // Don't delay after the last word
            if (i < words.Length - 1)
            {
                await Task.Delay(delayMs);
            }
        }

        // Complete streaming with final text
        string finalText = buffer.ToString();
        foreach (IResponseStreamWatcher watcher in watchers)
        {
            watcher.OnStreamComplete(finalText);
        }
    }

    private async Task StreamByCharacters(string text, List<IResponseStreamWatcher> watchers, int delayMs)
    {
        StringBuilder buffer = new StringBuilder();

        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];
            buffer.Append(character);

            // Update all watchers with this character
            foreach (IResponseStreamWatcher watcher in watchers)
            {
                watcher.OnStreamUpdate(character.ToString());
            }

            // Don't delay after the last character
            if (i < text.Length - 1)
            {
                await Task.Delay(delayMs);
            }
        }

        // Complete streaming with final text
        foreach (IResponseStreamWatcher watcher in watchers)
        {
            watcher.OnStreamComplete(buffer.ToString());
        }
    }
}