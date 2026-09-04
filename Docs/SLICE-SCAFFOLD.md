# HeroCity — SP Slice Scaffold v0

**Repo tip branch:** `scaffold/sp-slice-v0`  
**Engine:** Unity 6000.0.23f1 · **Net:** none (SP only)

## Boot → Play

1. Open project in Unity Hub.
2. Open `Assets/Scenes/Boot.unity` (or press Play from Boot).
3. **Play** → pick SURGE variant (Chainjack / Capacitor / Static Field) → Enter.
4. Walk Riverward S0→S5 (WASD, mouse look, Shift sprint, Space jump).
5. SURGE: **Q** grenade · **E** utility · **F** super (stubs).
6. Enter S5 Hideout volume → Nemesis intro placeholder beats (Blackout).

## Sockets

| Owner | Socket |
|-------|--------|
| Level | `RiverwardBootstrap` — full Riverward graybox-v0 (400×280, S0–S5, massing, hideout shell, Blackout props) |
| Systems | `SurgeController` variant stubs — fill numbers from SURGE-SLICE-KIT |
| Narrative | `NemesisIntroHook` beat strings — replace with VO/bark cards |


## Level pack (in-tree)

- `Docs/Riverward-graybox-v0.md` — meters, sockets S0–S5, hideout, Blackout props
- `Docs/Riverward-spine-v0.svg` + `Docs/Riverward-spine-v0.md` — spine map
- Runtime: `Assets/Scripts/Level/RiverwardBootstrap.cs` (pass 2 graybox)

## Out of slice

Online co-op, listen-server, visit-friend, dedicated host.

## SURGE Ability Kit v0
See `Docs/SURGE-ABILITY-KIT-v0.md`. CDs wired in `SurgeController`.
Stub order: Cap Mine+Cell dump → Arc Seed → Field Puck → supers last.

## Story sockets (SP)
C1 Call → C2 Pattern → C3 Trail → C4 Door → **N1** hideout intro (**The Watcher**) → C5 Aftertaste.
No co-op story.
