# Hub Demo Expand (SP slice)

CoS/QA pass: tip must be a real ~20 min Hub demo on `scaffold/sp-slice-v0`.

## Goals (this SHA)

1. **Real SURGE combat + encounter AI** — trash periodic strafe; elites HoldRange (7–12 m) + sphere projectiles.
2. **Arena spawn wiring** — S2→Arena_A (307,140), S3→Arena_B (160,178), S4→Arena_C (220,236), S5→Arena_D (280,240).
3. **S5 door volume + DoorUnlocked gate** — physical blocker until S4 wave clear.
4. **Objective HUD clarity** — multi-line [OBJ] + encounter status + beat label.
5. **Watcher → Blackout** — N1 VO ids (`VO.N1.Reveal` / `Clash` / `Grade` / `Exit`), disengage → `C5_Aftertaste`.

## DoorUnlocked

- `MissionChainController.DoorUnlocked` starts false.
- On `NotifyWaveCleared(S4)` → `DoorUnlocked = true`, objective *"Door unlocked — enter hideout (S5)"*, disables `Hideout_Door_Blocker`.
- `TryAdvance(S5)` and `Hideout_Door_Volume` (`requireDoorUnlocked`) refuse until unlocked (HUD: *"Clear C4 / unlock door first"*).
- Door volume at west gap (~240, 1.5, 240), size ~4×5×5 — not on the interior pad alone.

## Blackout + C5

- Boss GO named `Blackout`; logs use VO beat ids.
- At ≤30% HP: disengage (no full kill message), outro hook, `AdvanceToAftertaste()` → objective *"C5 Aftertaste — leave the hideout"*.
- `MissionNodeId.C5_Aftertaste = 6` with volume near door approach (~250, 0, 240).

## Waves

| Node | Trash | Elite |
|------|------:|------:|
| S0 | 4 | 0 |
| S1 | 5 | 1 |
| S2 | 6 | 1 |
| S3 | 7 | 2 |
| S4 | 8 | 2 |
| S5 | 5 | 1 |

SP only — no netcode. Do not touch Tag-game / Steam.
