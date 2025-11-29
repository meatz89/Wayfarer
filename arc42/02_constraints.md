# 2. Constraints

This section lists requirements constraining design and implementation decisions.

---

## 2.1 Technical Constraints

| Constraint | Background |
|------------|------------|
| .NET 8 / C# 12 | Team expertise; mature ecosystem with strong typing |
| Blazor Server (ServerPrerendered) | Single-player game benefits from server-side state; fast initial render |
| Single-threaded execution | Single-player, no multiplayer requirements |
| Browser-based delivery | Target platform is modern web browsers |

**Consequences:**
- Double-render lifecycle requires idempotent initialization
- No concurrent access patterns needed
- Performance optimization irrelevant (human reaction 200ms+ dominates)

---

## 2.2 Organizational Constraints

| Constraint | Background |
|------------|------------|
| JSON content format | Human-readable, version-controllable, AI-generatable |
| Atomic package loading | Content validated at parse-time; fail-fast on missing references |
| Package cohesion | Entities referencing each other must be co-located |

**Consequences:**
- Cross-package references require deferred resolution patterns
- Missing dependencies detected at startup, not runtime

---

## 2.3 Conventions

| Constraint | Background |
|------------|------------|
| Explicit types only | No `var`; type visible at declaration |
| `List<T>` for collections | No Dictionary/HashSet; maintainability over micro-optimization |
| Named methods | No anonymous delegates in backend; preserves stack traces |
| `int` for numbers | Intentional Numeric Design (GDD DDR-007) requires absolute modifiers and small values; integer arithmetic implements this |
| No entity instance IDs | Object references for relationships; categorical properties for queries |
| JSON field names = C# property names | No JsonPropertyName attributes; DTO structure is JSON schema |

---

## Related Documentation

- [01_introduction_and_goals.md](01_introduction_and_goals.md) — Quality goals these constraints support
- [04_solution_strategy.md](04_solution_strategy.md) — Decisions made within these constraints
