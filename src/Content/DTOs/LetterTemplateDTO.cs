using System.Collections.Generic;

/// <summary>
/// Data Transfer Object for deserializing letter template data from JSON.
/// Maps to the structure in letter_templates.json.
/// </summary>
public class LetterTemplateDTO
{
    public string Id { get; set; }
    public string Description { get; set; }
    public string TokenType { get; set; }
    public int MinDeadline { get; set; }
    public int MaxDeadline { get; set; }
    public int MinPayment { get; set; }
    public int MaxPayment { get; set; }
    
    // Letter category and requirements
    public string Category { get; set; } = "Basic";
    public int? MinTokensRequired { get; set; } = 3;
    
    // Optional fields
    public List<string> PossibleSenders { get; set; }
    public List<string> PossibleRecipients { get; set; }
    
    // Letter chain properties
    public List<string> UnlocksLetterIds { get; set; }
    public bool IsChainLetter { get; set; }
}