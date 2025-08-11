using System;
using System.Linq;
public class LetterQueue
{
    private Letter[] slots = new Letter[8];
    private const int MAX_SLOTS = 8;
    private const int MAX_WEIGHT = 12; // Maximum total weight capacity
    
    // Player tier for restricting which letters can be accepted
    // Set this from the game state when creating/updating the queue
    public TierLevel PlayerTier { get; set; } = TierLevel.T1;

    // Get letter at specific position (1-8)
    public Letter GetLetterAt(int position)
    {
        if (position < 1 || position > 8) return null;
        return slots[position - 1];
    }

    // Add letter to specific position (1-8) with weight and tier checking
    public bool AddLetter(Letter letter, int position)
    {
        if (position < 1 || position > 8) return false;
        if (letter == null) return false;
        
        // Special letters (permits, introductions) should not enter the queue
        // They go directly to inventory instead
        if (letter.IsSpecial)
        {
            // Log or handle special letter differently
            return false; // Don't add special letters to queue
        }
        
        // Check tier restriction - player can only accept letters at or below their tier
        if (letter.Tier > PlayerTier)
        {
            return false; // Letter tier too high for player
        }

        // Check if adding this letter would exceed weight capacity
        if (!CanAdd(letter)) return false;

        // Check if position is occupied
        if (slots[position - 1] != null) return false;

        slots[position - 1] = letter;
        letter.QueuePosition = position;
        return true;
    }

    // Check if a letter can be added based on weight and tier constraints
    public bool CanAdd(Letter letter)
    {
        if (letter == null) return false;
        
        // Special letters don't go in the queue
        if (letter.IsSpecial) return false;
        
        // Check tier restriction
        if (letter.Tier > PlayerTier) return false;
        
        int currentWeight = GetTotalWeight();
        return (currentWeight + letter.Weight) <= MAX_WEIGHT;
    }

    // Get total weight of all letters in queue
    public int GetTotalWeight()
    {
        return slots.Where(l => l != null).Sum(l => l.Weight);
    }

    // Get remaining weight capacity
    public int GetRemainingWeightCapacity()
    {
        return MAX_WEIGHT - GetTotalWeight();
    }

    // Remove letter at position and don't shift yet (for minimal POC)
    public void RemoveLetterAt(int position)
    {
        if (position < 1 || position > 8) return;

        Letter letter = slots[position - 1];
        if (letter != null)
        {
            letter.QueuePosition = 0;
            slots[position - 1] = null;
        }
    }

    // Deliver letter from position 1 (only allowed position for delivery)
    public Letter Deliver()
    {
        Letter letter = slots[0];
        if (letter != null)
        {
            RemoveLetterAt(1);
            // Shift all letters up one position
            ShiftLettersUp(2);
        }
        return letter;
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

    // Find lowest available position that can fit a letter by weight
    public int FindLowestAvailablePosition(Letter letter)
    {
        if (!CanAdd(letter)) return 0;
        
        for (int i = 0; i < 8; i++)
        {
            if (slots[i] == null)
                return i + 1;
        }
        return 0;
    }

    // Check if queue is full by weight or slots
    public bool IsFull()
    {
        // Queue is full if all slots are taken OR no more weight capacity
        return slots.All(slot => slot != null) || GetRemainingWeightCapacity() == 0;
    }

    // Check if position is empty
    public bool IsPositionEmpty(int position)
    {
        if (position < 1 || position > 8) return false;
        return slots[position - 1] == null;
    }

    // Get all letters in order (including nulls)
    public Letter[] GetAllLettersWithNulls()
    {
        return slots.ToArray();
    }
    
    // Get all non-null letters  
    public Letter[] GetAllLetters()
    {
        return slots.Where(l => l != null).ToArray();
    }

    // Count non-null letters
    public int GetLetterCount()
    {
        return slots.Count(slot => slot != null);
    }

    // Shift letters up when one is removed (compact queue)
    public void ShiftLettersUp(int fromPosition)
    {
        if (fromPosition < 2 || fromPosition > 8) return;
        
        // Shift all letters from fromPosition onwards up by one position
        for (int i = fromPosition - 1; i < 7; i++)
        {
            if (slots[i] != null)
            {
                slots[i - 1] = slots[i];
                slots[i - 1].QueuePosition = i;
                slots[i] = null;
            }
        }
    }

    // Reorder letters in queue (costs tokens in game logic)
    public bool Reorder(int fromPosition, int toPosition)
    {
        if (fromPosition < 1 || fromPosition > 8) return false;
        if (toPosition < 1 || toPosition > 8) return false;
        if (fromPosition == toPosition) return true;
        
        Letter letterToMove = slots[fromPosition - 1];
        if (letterToMove == null) return false;
        
        // Check if target position is empty
        if (slots[toPosition - 1] != null)
        {
            // Swap positions
            Letter temp = slots[toPosition - 1];
            slots[toPosition - 1] = letterToMove;
            slots[fromPosition - 1] = temp;
            
            letterToMove.QueuePosition = toPosition;
            temp.QueuePosition = fromPosition;
        }
        else
        {
            // Simple move to empty position
            slots[toPosition - 1] = letterToMove;
            slots[fromPosition - 1] = null;
            letterToMove.QueuePosition = toPosition;
        }
        
        return true;
    }

    // Get letters that are expiring soon (deadline <= hours)
    public Letter[] GetExpiringLetters(int hours)
    {
        return slots
            .Where(letter => letter != null && letter.DeadlineInHours <= hours)
            .OrderBy(letter => letter.DeadlineInHours)
            .ToArray();
    }
    
    // Get letters from a specific NPC
    public Letter[] GetLettersFrom(string npcId)
    {
        return slots
            .Where(letter => letter != null && letter.SenderId == npcId)
            .ToArray();
    }

    // Get active letters (for UI display)
    public Letter[] GetActiveLetters()
    {
        return GetAllLetters();
    }
    
    // Property for easier access
    public Letter[] Letters => GetAllLetters();
    
    // Properties for UI display
    public int MaxWeight => MAX_WEIGHT;
    public int MaxSlots => MAX_SLOTS;
}