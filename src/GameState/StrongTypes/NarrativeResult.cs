public class NarrativeResult
{
public string Message { get; init; }
public string Severity { get; init; }

public NarrativeResult(string message, string severity)
{
    Message = message;
    Severity = severity;
}
}

public class TokenNarrativeResult
{
public string Reaction { get; init; }
public string Summary { get; init; }

public TokenNarrativeResult(string reaction, string summary)
{
    Reaction = reaction;
    Summary = summary;
}
}

public class MilestoneNarrativeResult
{
public string Milestone { get; init; }
public string Additional { get; init; }

public MilestoneNarrativeResult(string milestone, string additional)
{
    Milestone = milestone;
    Additional = additional;
}
}

public class MorningNarrativeResult
{
public string Morning { get; init; }
public string LetterCount { get; init; }
public string Severity { get; init; }

public MorningNarrativeResult(string morning, string letterCount, string severity)
{
    Morning = morning;
    LetterCount = letterCount;
    Severity = severity;
}
}

public class TransitionNarrativeResult
{
public string Transition { get; init; }
public string Action { get; init; }

public TransitionNarrativeResult(string transition, string action)
{
    Transition = transition;
    Action = action;
}
}