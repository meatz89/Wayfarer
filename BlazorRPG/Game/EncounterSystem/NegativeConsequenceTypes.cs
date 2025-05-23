public enum NegativeConsequenceTypes
{
    None,
    ProgressLoss,           // "Lose 1 Progress Marker"
    FocusLoss,             // "Lose 1 Focus Point"
    TokenDisruption,       // "Discard X random tokens" OR "This generation produces 1 fewer token"
    ThresholdIncrease,     // "Success thresholds increase by 1"
    ConversionReduction    // "This conversion yields 1 less Progress"
}