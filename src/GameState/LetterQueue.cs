using System;
using System.Linq;
public class LetterQueue
{
    private Letter[] slots = new Letter[8];
    
    // Get letter at specific position (1-8)
    public Letter GetLetterAt(int position)
    {
        if (position < 1 || position > 8) return null;
        return slots[position - 1];
    }
    
    // Add letter to specific position (1-8)
    public void AddLetter(Letter letter, int position)
    {
        if (position < 1 || position > 8) return;
        if (letter == null) return;
        
        slots[position - 1] = letter;
        letter.QueuePosition = position;
    }
    
    // Remove letter at position and don't shift yet (for minimal POC)
    public void RemoveLetterAt(int position)
    {
        if (position < 1 || position > 8) return;
        
        var letter = slots[position - 1];
        if (letter != null)
        {
            letter.QueuePosition = 0;
            slots[position - 1] = null;
        }
    }
    
    // Find first empty slot position (returns 0 if queue is full)
    public int FindFirstEmptySlot()
    {
        for (int i = 0; i < 8; i++)
        {
            if (slots[i] == null)
                return i + 1;
        }
        return 0; // Queue is full
    }
    
    // Check if queue is full
    public bool IsFull()
    {
        return slots.All(slot => slot != null);
    }
    
    // Check if position is empty
    public bool IsPositionEmpty(int position)
    {
        if (position < 1 || position > 8) return false;
        return slots[position - 1] == null;
    }
    
    // Get all letters in order
    public Letter[] GetAllLetters()
    {
        return slots.ToArray();
    }
    
    // Count non-null letters
    public int GetLetterCount()
    {
        return slots.Count(slot => slot != null);
    }
    
    // For future: shift letters up when one is removed
    public void ShiftLettersUp(int fromPosition)
    {
        // Not implemented for minimal POC
        // Will implement in full version
    }
    
    // Get letters that are expiring soon (deadline <= days)
    public Letter[] GetExpiringLetters(int days)
    {
        return slots
            .Where(letter => letter != null && letter.Deadline <= days)
            .OrderBy(letter => letter.Deadline)
            .ToArray();
    }
}