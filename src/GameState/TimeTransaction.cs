using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.StateContainers;

namespace Wayfarer.GameState;

/// <summary>
/// Represents an atomic time-based transaction that can include multiple effects.
/// Ensures all operations succeed or none are applied.
/// </summary>
public class TimeTransaction
{
    private readonly TimeModel _timeModel;
    private readonly List<ITimeBasedEffect> _effects = new();
    private readonly Dictionary<string, object> _context = new();
    private int _totalHoursCost;
    private string _description;
    private bool _requiresActiveHours = true;

    public TimeTransaction(TimeModel timeModel)
    {
        _timeModel = timeModel ?? throw new ArgumentNullException(nameof(timeModel));
    }

    /// <summary>
    /// Adds hours to the transaction cost.
    /// </summary>
    public TimeTransaction WithHours(int hours, string description = null)
    {
        if (hours <= 0)
            throw new ArgumentException("Hours must be positive", nameof(hours));

        _totalHoursCost += hours;
        
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
    public TimeTransaction WithContext(string key, object value)
    {
        _context[key] = value;
        return this;
    }

    /// <summary>
    /// Sets whether this transaction requires active hours (default: true).
    /// </summary>
    public TimeTransaction RequireActiveHours(bool require)
    {
        _requiresActiveHours = require;
        return this;
    }

    /// <summary>
    /// Validates whether the transaction can be executed.
    /// </summary>
    public TransactionValidation CanExecute()
    {
        var validation = new TransactionValidation();

        // Check if we have enough time
        if (_requiresActiveHours && !_timeModel.CanPerformAction(_totalHoursCost))
        {
            validation.AddError($"Insufficient active hours. Need {_totalHoursCost} hours, have {_timeModel.ActiveHoursRemaining} remaining.");
            return validation;
        }

        // Validate all effects
        foreach (var effect in _effects)
        {
            var effectValidation = effect.Validate(_timeModel.CurrentState, _context);
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
        var validation = CanExecute();
        if (!validation.IsValid)
        {
            return TimeTransactionResult.Failure(validation.Errors);
        }

        var completedEffects = new List<ITimeBasedEffect>();
        var effectResults = new List<EffectResult>();

        try
        {
            // Advance time first
            var timeAdvancement = _totalHoursCost > 0 
                ? _timeModel.AdvanceTime(_totalHoursCost) 
                : null;

            // Execute all effects in order
            foreach (var effect in _effects)
            {
                var result = effect.Apply(_timeModel.CurrentState, _context);
                effectResults.Add(result);
                
                if (!result.Success)
                {
                    throw new TimeTransactionException($"Effect '{effect.GetType().Name}' failed: {result.Message}");
                }

                completedEffects.Add(effect);

                // If effect blocks subsequent effects, stop here
                if (result.BlocksSubsequentEffects)
                    break;
            }

            return TimeTransactionResult.Success(timeAdvancement, effectResults, _description);
        }
        catch (Exception ex)
        {
            // Rollback completed effects in reverse order
            foreach (var effect in completedEffects.AsEnumerable().Reverse())
            {
                try
                {
                    effect.Rollback(_timeModel.CurrentState, _context);
                }
                catch (Exception rollbackEx)
                {
                    // Log rollback failure but continue with other rollbacks
                    Console.WriteLine($"Rollback failed for {effect.GetType().Name}: {rollbackEx.Message}");
                }
            }

            return TimeTransactionResult.Failure(new[] { ex.Message });
        }
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
    EffectValidation Validate(TimeState currentTime, Dictionary<string, object> context);

    /// <summary>
    /// Applies the effect.
    /// </summary>
    EffectResult Apply(TimeState currentTime, Dictionary<string, object> context);

    /// <summary>
    /// Rolls back the effect if the transaction fails.
    /// </summary>
    void Rollback(TimeState currentTime, Dictionary<string, object> context);
}

/// <summary>
/// Result of a time transaction execution.
/// </summary>
public class TimeTransactionResult
{
    public bool Success { get; init; }
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
            Success = true,
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
            Success = false,
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

    public static EffectValidation Valid() => new EffectValidation { IsValid = true };
    public static EffectValidation Invalid(string message) => new EffectValidation { IsValid = false, Message = message };
}

/// <summary>
/// Result of applying an effect.
/// </summary>
public class EffectResult
{
    public bool Success { get; init; }
    public string Message { get; init; }
    public bool BlocksSubsequentEffects { get; init; }
    public Dictionary<string, object> OutputData { get; init; } = new();

    public static EffectResult Succeeded(string message = null) => new EffectResult 
    { 
        Success = true, 
        Message = message 
    };

    public static EffectResult Failed(string message) => new EffectResult 
    { 
        Success = false, 
        Message = message 
    };
}

/// <summary>
/// Exception thrown when a time transaction fails.
/// </summary>
public class TimeTransactionException : Exception
{
    public TimeTransactionException(string message) : base(message) { }
    public TimeTransactionException(string message, Exception innerException) : base(message, innerException) { }
}