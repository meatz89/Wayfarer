# 11. Risks and Technical Debt

This section documents known technical risks and accumulated technical debt.

---

## 11.1 Technical Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| **WebSocket Disconnection** | Medium | High - gameplay interrupted | No offline mode; reconnection requires state reload; consider state persistence |
| **Content Validation Gaps** | Medium | High - soft-locks possible | Parser validation catches structural issues; edge cases may slip through |
| **Procedural Generation Balance** | Medium | Medium - player frustration | Categorical translation normalizes difficulty; extreme combinations untested |
| **Memory Growth Over Sessions** | Low | Medium - browser slowdown | Query-time action deletion helps; long sessions untested |
| **Blazor ServerPrerendered Complexity** | Low | Medium - subtle init bugs | Idempotent initialization pattern; requires discipline |

### Risk Details

**WebSocket Disconnection:**
Blazor Server requires persistent WebSocket. Network interruption loses all ephemeral state. Current architecture has no reconnection strategy—page reload required, losing in-progress scene state.

**Content Validation Gaps:**
Parser validates JSON structure and required fields. Cannot validate semantic correctness—a scene could have choices that all require impossible stat combinations. Requires manual playtesting for complex content.

**Procedural Generation Balance:**
Categorical translation (Friendly × Premium = concrete values) works for expected ranges. Extreme combinations (Hostile × Premium × Dominant NPC at Budget × Dangerous location) may produce unbalanced encounters.

---

## 11.2 Accepted Limitations

These are conscious design decisions, not debt:

| Limitation | Rationale |
|------------|-----------|
| No offline play | Blazor Server chosen for simplicity; acceptable for target audience |
| No multiplayer | Single-player design decision; architecture doesn't preclude future addition |
| No mobile optimization | Browser-based desktop target; mobile would require UI redesign |
| No save/load | GameWorld serialization planned but not implemented |

---

## Related Documentation

- [09_architecture_decisions.md](09_architecture_decisions.md) — Decisions and their consequences
- [02_constraints.md](02_constraints.md) — Constraints that may create debt
- [10_quality_requirements.md](10_quality_requirements.md) — Quality scenarios for validation
