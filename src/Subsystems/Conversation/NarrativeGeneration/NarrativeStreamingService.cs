using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Service for streaming narrative content with typewriter-style display.
/// Uses IAsyncEnumerable to provide Blazor-safe streaming without threading issues.
/// </summary>
public class NarrativeStreamingService
{
    /// <summary>
    /// Streams text content in chunks for typewriter-style display.
    /// </summary>
    /// <param name="fullText">The complete text to stream</param>
    /// <param name="chunkSize">Number of words per chunk</param>
    /// <param name="delayMs">Delay between chunks in milliseconds</param>
    /// <param name="cancellationToken">Cancellation token for stopping stream</param>
    /// <returns>Stream of narrative chunks</returns>
    public async IAsyncEnumerable<NarrativeChunk> StreamNarrativeAsync(
        string fullText,
        int chunkSize = 3,
        int delayMs = 50,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fullText))
        {
            yield return new NarrativeChunk
            {
                Text = string.Empty,
                IsComplete = true,
                IsDialogue = false
            };
            yield break;
        }

        string[] words = fullText.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        StringBuilder currentChunk = new StringBuilder();
        int wordsInCurrentChunk = 0;
        int totalWordsProcessed = 0;

        for (int i = 0; i < words.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (currentChunk.Length > 0)
            {
                currentChunk.Append(' ');
            }
            currentChunk.Append(words[i]);
            wordsInCurrentChunk++;
            totalWordsProcessed++;

            bool isLastWord = (i == words.Length - 1);
            bool chunkIsFull = (wordsInCurrentChunk >= chunkSize);

            if (chunkIsFull || isLastWord)
            {
                yield return new NarrativeChunk
                {
                    Text = currentChunk.ToString(),
                    IsComplete = isLastWord,
                    IsDialogue = DetermineIsDialogue(fullText)
                };

                if (!isLastWord)
                {
                    await Task.Delay(delayMs, cancellationToken);
                }

                currentChunk.Clear();
                wordsInCurrentChunk = 0;
            }
        }
    }

    /// <summary>
    /// Streams narrative content from an INarrativeProvider.
    /// Handles both AI streaming and static text streaming.
    /// </summary>
    /// <param name="provider">The narrative provider to generate content</param>
    /// <param name="state">Current conversation state</param>
    /// <param name="npcData">NPC data for generation</param>
    /// <param name="cards">Available cards for the player</param>
    /// <param name="chunkSize">Words per chunk for streaming</param>
    /// <param name="delayMs">Delay between chunks</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream of narrative chunks</returns>
    public async IAsyncEnumerable<NarrativeChunk> StreamFromProviderAsync(
        INarrativeProvider provider,
        SocialChallengeState state,
        NPCData npcData,
        CardCollection cards,
        int chunkSize = 3,
        int delayMs = 50,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Check provider availability using async method
        bool isAvailable = await provider.IsAvailableAsync();
        if (!isAvailable)
        {
            yield return new NarrativeChunk
            {
                Text = $"[{provider.GetProviderType()} is not available]",
                IsComplete = true,
                IsDialogue = false
            };
            yield break;
        }

        // First generate NPC dialogue
        NarrativeOutput output = await provider.GenerateNPCDialogueAsync(state, npcData, cards);

        // Stream NPC dialogue first if available
        if (!string.IsNullOrWhiteSpace(output.NPCDialogue))
        {
            await foreach (NarrativeChunk chunk in StreamNarrativeAsync(output.NPCDialogue, chunkSize, delayMs, cancellationToken))
            {
                chunk.IsDialogue = true;
                yield return chunk;
            }

            // Add a pause between dialogue and narrative text
            if (!string.IsNullOrWhiteSpace(output.NarrativeText))
            {
                await Task.Delay(delayMs * 2, cancellationToken);
            }
        }

        // Stream narrative text if available
        if (!string.IsNullOrWhiteSpace(output.NarrativeText))
        {
            await foreach (NarrativeChunk chunk in StreamNarrativeAsync(output.NarrativeText, chunkSize, delayMs, cancellationToken))
            {
                chunk.IsDialogue = false;
                yield return chunk;
            }
        }

        // Stream progression hint if available
        if (!string.IsNullOrWhiteSpace(output.ProgressionHint))
        {
            await Task.Delay(delayMs * 2, cancellationToken);

            await foreach (NarrativeChunk chunk in StreamNarrativeAsync($"[{output.ProgressionHint}]", chunkSize, delayMs, cancellationToken))
            {
                chunk.IsDialogue = false;
                yield return chunk;
            }
        }

        // Note: Card narratives are generated separately and not streamed here
        // They are handled by a separate call to GenerateCardNarrativesAsync
    }

    /// <summary>
    /// Streams a single narrative output immediately without delays.
    /// Useful for non-streaming providers or when immediate display is needed.
    /// </summary>
    /// <param name="output">The narrative output to stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream of complete narrative chunks</returns>
    public async IAsyncEnumerable<NarrativeChunk> StreamOutputImmediateAsync(
        NarrativeOutput output,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!string.IsNullOrWhiteSpace(output.NPCDialogue))
        {
            yield return new NarrativeChunk
            {
                Text = output.NPCDialogue,
                IsComplete = string.IsNullOrWhiteSpace(output.NarrativeText) && string.IsNullOrWhiteSpace(output.ProgressionHint),
                IsDialogue = true
            };
        }

        if (!string.IsNullOrWhiteSpace(output.NarrativeText))
        {
            yield return new NarrativeChunk
            {
                Text = output.NarrativeText,
                IsComplete = string.IsNullOrWhiteSpace(output.ProgressionHint),
                IsDialogue = false
            };
        }

        if (!string.IsNullOrWhiteSpace(output.ProgressionHint))
        {
            yield return new NarrativeChunk
            {
                Text = $"[{output.ProgressionHint}]",
                IsComplete = true,
                IsDialogue = false
            };
        }

        await Task.CompletedTask; // Satisfy async requirement
    }

    /// <summary>
    /// Determines if text appears to be dialogue based on content analysis.
    /// Simple heuristic: text in quotes or starting with common dialogue patterns.
    /// </summary>
    /// <param name="text">Text to analyze</param>
    /// <returns>True if text appears to be dialogue</returns>
    private bool DetermineIsDialogue(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        string trimmed = text.Trim();

        // Check for quoted text
        if (trimmed.StartsWith("\"") && trimmed.EndsWith("\""))
            return true;

        if (trimmed.StartsWith("'") && trimmed.EndsWith("'"))
            return true;

        // Check for common dialogue patterns
        if (trimmed.StartsWith("I ") || trimmed.StartsWith("You ") ||
            trimmed.StartsWith("We ") || trimmed.StartsWith("They "))
            return true;

        // Default to false for narrative descriptions
        return false;
    }
}