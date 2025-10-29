/// <summary>
/// Strongly typed context for time-based transactions
/// </summary>
public class TransactionContext
{
    public string VenueId { get; set; }
    public string NpcId { get; set; }
    public string DestinationId { get; set; }
    public int? TravelDistance { get; set; }
    public string ActionType { get; set; }
    public string ItemId { get; set; }
    public int? ItemQuantity { get; set; }
    public ResourceType? ResourceType { get; set; }
    public int? ResourceAmount { get; set; }
}

/// <summary>
/// Strongly typed output data from effects
/// </summary>
public class EffectOutputData
{
    public string ResultType { get; set; }
    public int? ValueChanged { get; set; }
    public string NewState { get; set; }
    public string UnlockedFeature { get; set; }
    public List<string> GeneratedItems { get; set; } = new();
    public string ErrorReason { get; set; }
}

/// <summary>
/// Represents an atomic time-based transaction that can include multiple effects.
/// Ensures all operations succeed or none are applied.
/// </summary>
public class TimeTransaction
{
    private readonly TimeModel _timeModel;
    private readonly List<ITimeBasedEffect> _effects = new();
    private readonly TransactionContext _context = new();
    private int _totalSegmentsCost;
    private string _description;
    private bool _requiresActiveSegments = true;

    public TimeTransaction(TimeModel timeModel)
    {
        _timeModel = timeModel ?? throw new ArgumentNullException(nameof(timeModel));
    }

    /// <summary>
    /// Adds segments to the transaction cost.
    /// </summary>
    public TimeTransaction WithSegments(int segments, string description)
    {
        if (segments <= 0)
            throw new ArgumentException("Segments must be positive", nameof(segments));

        _totalSegmentsCost += segments;

        if (!string.IsNullOrEmpty(description))
            _description = string.IsNullOrEmpty(_description) ? description : $"{_description}; {description}";

        return this;
    }

    /// <summary>
    /// Adds an effect to be executed after time advancement.
    /// </summary>
    public TimeTransaction WithEffect(ITimeBasedEffect effect)
    {
        if (effect == null)
            throw new ArgumentNullException(nameof(effect));

        _effects.Add(effect);
        return this;
    }

    /// <summary>
    /// Adds context data for effects to use.
    /// </summary>
    public TimeTransaction WithContext(Action<TransactionContext> configureContext)
    {
        if (configureContext != null)
            configureContext.Invoke(_context);
        return this;
    }

    /// <summary>
    /// Sets whether this transaction requires active segments (default: true).
    /// </summary>
    public TimeTransaction RequireActiveSegments(bool require)
    {
        _requiresActiveSegments = require;
        return this;
    }

    /// <summary>
    /// Validates whether the transaction can be executed.
    /// </summary>
    public TransactionValidation CanExecute()
    {
        TransactionValidation validation = new TransactionValidation();

        // Check if we have enough time
        if (_requiresActiveSegments && !_timeModel.CanPerformAction(_totalSegmentsCost))
        {
            validation.AddError($"Insufficient active segments. Need {_totalSegmentsCost} segments, have {_timeModel.ActiveSegmentsRemaining} remaining.");
            return validation;
        }

        // Validate all effects
        foreach (ITimeBasedEffect effect in _effects)
        {
            EffectValidation effectValidation = effect.Validate(_timeModel.CurrentState, _context);
            if (!effectValidation.IsValid)
            {
                validation.AddError($"Effect '{effect.GetType().Name}' validation failed: {effectValidation.Message}");
            }
        }

        return validation;
    }

    /// <summary>
    /// Executes the transaction atomically.
    /// </summary>
    public TimeTransactionResult Execute()
    {
        TransactionValidation validation = CanExecute();
        if (!validation.IsValid)
        {
            return TimeTransactionResult.Failure(validation.Errors.ToArray());
        }

        List<ITimeBasedEffect> completedEffects = new List<ITimeBasedEffect>();
        List<EffectResult> effectResults = new List<EffectResult>();

        // Advance time first
        TimeAdvancementResult timeAdvancement = _totalSegmentsCost > 0
            ? _timeModel.AdvanceSegments(_totalSegmentsCost)
            : null;

        // Execute all effects in order
        foreach (ITimeBasedEffect effect in _effects)
        {
            EffectResult result = effect.Apply(_timeModel.CurrentState, _context);
            effectResults.Add(result);

            if (!result.Success)
            {
                // Rollback completed effects in reverse order before returning failure
                foreach (ITimeBasedEffect? completedEffect in completedEffects.AsEnumerable().Reverse())
                {
                    completedEffect.Rollback(_timeModel.CurrentState, _context);
                }
                return TimeTransactionResult.Failure(new[] { result.Message });
            }

            completedEffects.Add(effect);

            // If effect blocks subsequent effects, stop here
            if (result.BlocksSubsequentEffects)
                break;
        }

        return TimeTransactionResult.Success(timeAdvancement, effectResults, _description);
    }
}

/// <summary>
/// Interface for effects that occur as part of time transactions.
/// </summary>
public interface ITimeBasedEffect
{
    /// <summary>
    /// Validates whether this effect can be applied.
    /// </summary>
    EffectValidation Validate(TimeState currentTime, TransactionContext context);

    /// <summary>
    /// Applies the effect.
    /// </summary>
    EffectResult Apply(TimeState currentTime, TransactionContext context);

    /// <summary>
    /// Rolls back the effect if the transaction fails.
    /// </summary>
    void Rollback(TimeState currentTime, TransactionContext context);
}

/// <summary>
/// Result of a time transaction execution.
/// </summary>
public class TimeTransactionResult
{
    public bool IsSuccess { get; init; }
    public TimeAdvancementResult TimeAdvancement { get; init; }
    public List<EffectResult> EffectResults { get; init; }
    public string Description { get; init; }
    public string[] Errors { get; init; }

    public static TimeTransactionResult Success(
        TimeAdvancementResult timeAdvancement,
        List<EffectResult> effectResults,
        string description)
    {
        return new TimeTransactionResult
        {
            IsSuccess = true,
            TimeAdvancement = timeAdvancement,
            EffectResults = effectResults,
            Description = description,
            Errors = Array.Empty<string>()
        };
    }

    public static TimeTransactionResult Failure(string[] errors)
    {
        return new TimeTransactionResult
        {
            IsSuccess = false,
            Errors = errors,
            EffectResults = new List<EffectResult>()
        };
    }
}

/// <summary>
/// Validation result for transactions.
/// </summary>
public class TransactionValidation
{
    private readonly List<string> _errors = new();

    public bool IsValid => _errors.Count == 0;

    public IReadOnlyList<string> Errors => _errors;

    public void AddError(string error)
    {
        _errors.Add(error);
    }
}

/// <summary>
/// Validation result for individual effects.
/// </summary>
public class EffectValidation
{
    public bool IsValid { get; init; }
    public string Message { get; init; }

    public static EffectValidation Valid()
    {
        return new EffectValidation { IsValid = true };
    }

    public static EffectValidation Invalid(string message)
    {
        return new EffectValidation { IsValid = false, Message = message };
    }
}

/// <summary>
/// Result of applying an effect.
/// </summary>
public class EffectResult
{
    public bool Success { get; init; }
    public string Message { get; init; }
    public bool BlocksSubsequentEffects { get; init; }
    public EffectOutputData OutputData { get; init; } = new();

    public static EffectResult Succeeded(string message)
    {
        return new EffectResult
        {
            Success = true,
            Message = message
        };
    }

    public static EffectResult Failed(string message)
    {
        return new EffectResult
        {
            Success = false,
            Message = message
        };
    }
}

/// <summary>
/// Exception thrown when a time transaction fails.
/// </summary>
public class TimeTransactionException : Exception
{
    public TimeTransactionException(string message) : base(message) { }
    public TimeTransactionException(string message, Exception innerException) : base(message, innerException) { }
}