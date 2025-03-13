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
    ReduceConcentrationByPressure,   // Reduce concentration by X where X is current pressure
    ReduceReputationByPressure,      // Reduce reputation by X where X is current pressure
    ReduceHealthByApproachValue,     // Reduce health by X where X is approach tag value
    ReduceConcentrationByApproachValue,  // Reduce concentration by X where X is approach tag value
    ReduceReputationByApproachValue,      // Reduce reputation by X where X is approach tag value
    ReduceConfidenceByPressure,
    ReduceHealthByApproach
}