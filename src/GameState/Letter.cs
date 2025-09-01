using System;
using System.Collections.Generic;

/// <summary>
/// Physical letter object carried in satchel
/// Contains only the physical properties of the letter itself
/// </summary>
public class Letter
{
    // Identity
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // Physical properties
    public int Size { get; set; } = 1; // Physical size for satchel capacity
    public LetterPhysicalProperties PhysicalProperties { get; set; } = LetterPhysicalProperties.None;

    // Content metadata (what's written on the letter)
    public string SenderName { get; set; } = "";
    public string RecipientName { get; set; } = "";
    public LetterSpecialType SpecialType { get; set; } = LetterSpecialType.None;

    // Special letter data - unlock properties are only on physical Letters
    public string UnlocksNPCId { get; set; } = "";  // For Introduction letters
    public string UnlocksRouteId { get; set; } = "";  // For Access Permit letters - unlocks routes, not locations directly

    // Visual appearance
    public bool IsSealed { get; set; } = true;
    public bool IsUrgent { get; set; } = false;
    public bool IsOfficial { get; set; } = false;
    public bool IsPersonal { get; set; } = false;

    // Condition
    public LetterCondition Condition { get; set; } = LetterCondition.Pristine;

    public Letter()
    {
        Id = Guid.NewGuid().ToString();
    }

    public Letter(string senderId, string recipientId)
    {
        Id = Guid.NewGuid().ToString();
        SenderName = senderId;
        RecipientName = recipientId;
    }

    // Helper properties
    public bool IsSpecial => SpecialType != LetterSpecialType.None;
    public bool IsBulky => PhysicalProperties.HasFlag(LetterPhysicalProperties.Bulky);
    public bool IsFragile => PhysicalProperties.HasFlag(LetterPhysicalProperties.Fragile);
}

public enum LetterCondition
{
    Pristine,
    Good,
    Worn,
    Damaged,
    Deteriorating
}