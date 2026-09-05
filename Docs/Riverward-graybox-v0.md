# Riverward graybox v0 — Eng handoff
**Owner:** Level  
**Status:** IN UNITY — `RiverwardBootstrap` remetered to this brief (S0–S5 + massing + hideout shell + Blackout props). SP only. No 2p density. Patrol sim = cut if tight.  
**Refs:** `CITY-BLOCKOUT-THESIS-v0.md` (v0.3) · `Riverward-spine-v0.svg` · `Riverward-spine-v0.md` · Narrative beat sheet  
**Date:** 2026-09-04

---

## Envelope
- Playable **400 × 280 m**. Origin **SW**. +X east, +Z north, +Y up.
- Water strip Z[0, 40]. Boardwalk Z[40, 80]. Two-Flats Z[80, 160]. Junction Z[160, 200]. Warehouse Z[200, 280].
- Street grid: N–S avenues at X ≈ 67, 200, 330 (10 m carriage). E–W at Z ≈ 80, 160, 200.

## Sockets (empty volumes + names)
| ID | Position (X,Z) | Size (m) | Narrative |
|----|----------------|----------|-----------|
| `Socket_S0` | (200, 60) | 12×12 | C1 Call — ferry/boardwalk |
| `Socket_S1` | (107, 113) | 10×10 | C1b — bodega/stoop |
| `Socket_S2` | (307, 127) | 10×10 | C2 Pattern — alley / 1–2F roof |
| `Socket_S3` | (200, 180) | 16×16 | C3 Funnel — precinct-edge Junction |
| `Socket_S4` | (200, 233) | 12×12 | C4 Door — dock approach |
| `Socket_S5` | (246, 240) door apron | **14×12** (was 4×4 missable) | N1 — hideout west-door threshold |
| `Socket_C5_Exit` | (200, 200) | 8×8 | C5 Aftertaste — Junction/Strip |

Mission path: S0→S1→S2→S3→S4→S5→C5_Exit.

## Massing (primitives OK)
| Piece | Footprint | Notes |
|-------|-----------|--------|
| Water | X[0,400] Z[0,40] Y=−1 | Plane / volume |
| Pier landmark L1 | X[340,380] Z[45,55] | Boardwalk |
| Two-flat rows | 2–3 story boxes along Z[90,150] | Gaps = alleys ~8 m at X≈93, 200, 307 |
| Bodega stub S1 | 8×8×4 at (107,113) | Stoop 1 m up |
| Local roof S2 | 12×10 top at Y=6–7 near (307,127) | Stairs/mantle TBD Systems |
| Junction plaza | X[120,280] Z[160,200] | Open; one arch/gate north to warehouses |
| Precinct stoop | 10×6×3 on plaza west | Civic *edge* presence only |
| Warehouse blocks | 2–4 story along Z[210,270] | |
| **Hideout** | X[240,347] Z[213,267] | Interior shell + roof deck Y≈10; door at S5 |
| Elevator L3 | ~8×8×40 at (347, 250) | Landmark silhouette |

## Hideout staging — Blackout (N1)
If free in graybox, place readable props (no final art):
- **Blacked block** — one building face / panel cluster with “dead” windows (darker boxes) on hideout exterior or approach
- **Fused breaker** — wall prop near S5 door or interior entry (socket `Prop_Breaker_Blackout`)
- **Dead pier neon** — unlit sign stub on boardwalk L1 *or* a killed neon echo inside hideout (same silhouette language)

Calling card should read in ≤2 s at S5 threshold. Don’t invent anti-Surge mechanics — dressing only.

## Cuts
No second district · no skyline · no patrol sim required · no traffic · no crowd · max enterables = hideout (+ optional tiny bodega shell).

## Unity pass 2 (2026-09-04)
Spine path ribbons · S2 stair/mantle stubs · S5 door frame/beacon for ~40 m read from S4 · denser warehouse flank · approach chevrons.

## Unity pass 3 (2026-09-04)
Ferry/S0 Call landmark · alley connectors X≈93/200/307 · hideout interior hall/rooms + roof stair stubs · bodega interior hint.

## S5 door fix (2026-09-05)
Pad moved to west-door threshold ~(246,240), **14×12** apron + wider door (8 m) + taller beacon for ~40 m S4 read. Interior (293,240) remains hideout center — volume fires at door.

## Success
Walk S0→S5 using landmarks only in <150 s without combat. Door readable from S4 at 40 m. Water always reorients.
