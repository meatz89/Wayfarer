public enum StrategicEffectTypes
{
    // Momentum effects
    AddMomentumToApproach,
    AddMomentumToFocus,
    ReduceMomentumFromApproach,
    ReduceMomentumFromFocus,
    AddMomentumOnActivation,
    AddMomentumPerTurn,

    // Pressure effects
    ReducePressurePerTurn,
    AddPressurePerTurn,
    ReducePressureFromApproach,
    ReducePressureFromFocus,
    AddPressureFromApproach,
    AddPressureFromFocus,
    ReducePressureOnActivation,

    // New persistent resource effects
    ReduceHealthByPressure,          // Reduce health by X where X is current pressure
    ReduceFocusByPressure,   // Reduce concentration by X where X is current pressure
    ReduceConfidenceByPressure,      // Reduce reputation by X where X is current pressure
    ReduceHealthByApproachValue,     // Reduce health by X where X is approach tag value
    ReduceFocusByApproachValue,  // Reduce concentration by X where X is approach tag value
    ReduceConfidenceByApproachValue,      // Reduce reputation by X where X is approach tag value
    ReduceHealthByApproach
}