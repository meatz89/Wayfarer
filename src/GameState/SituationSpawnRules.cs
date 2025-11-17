// ADR-007: SituationSpawnRules class DELETED - never instantiated (dead code)
// Class defined but never used anywhere in codebase
//
// DELETED PROPERTIES (1 ID property):
//   - public string InitialSituationId { get; set; } (line 21)
//
// DELETED PROPERTIES (non-ID):
//   - SpawnPattern Pattern
//   - List<SituationTransition> Transitions
//   - string CompletionCondition
//
// Verification: grep ": SituationSpawnRules\|<SituationSpawnRules" = 0 results
// Class never used as a type anywhere in codebase
