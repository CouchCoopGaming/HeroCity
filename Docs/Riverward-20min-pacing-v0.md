# Riverward ~20 min SP demo pacing v0
**Owner:** Level · **Status:** GREENLIT (NOT STANDBY) — paper + Arena_A–D shipped
**QA:** tip `dc5cc7c` · `RiverwardBootstrap.Build20MinPack` → `Pass4_20min` / `Arena_A`…`Arena_D` + `Nav_*`  
**Refs:** `Riverward-graybox-v0.md` · `Riverward-spine-v0.md` · Narrative slice beats  
**Constraints:** SP only · no new districts · no 2p density · no teleport stubs · Riverward 400×280 only

## Goal
S0→S5 plays as a **~20 minute** SP demo: walkable spine, fight arenas, nav bake envelopes. Same ward.

## Time-per-socket + fight beats
| Socket | Time | Travel / space | Fight beat (Level space; Systems numbers TBD) |
|--------|------|----------------|-----------------------------------------------|
| **S0** Call | **1.5 min** | Ferry + boardwalk loop to pier L1 | **None** — reorient on water; optional bark only |
| **S1** Street teach | **2.5 min** | Bodega/stoop + alley spur toward X≈93 | **Soft contact** — 1–2 chaff in stoop pocket (no named arena); teach cover at stoop |
| **S2** Pattern | **3.5 min** | Alley X≈307 + roof pad + stairs | **Arena_A** — first real fight: alley court, waist cover, mantle rim; Surge grenade-friendly |
| **S3** Funnel | **2.5 min** | Junction plaza circuit, precinct edge west | **Arena_B** — open plaza slice; push/pull space; cut first if over time |
| **S4** Door | **3.0 min** | Warehouse lane S4→west door (~40 m read) | **Arena_C** — linear lane + one side bay; cover ribs; door beacon stays readable |
| **S5** N1 Hideout | **5.0 min** | Interior hall/rooms → optional roof | **Arena_D** — interior rooms; Blackout calling card at threshold; roof as escape/teach beat |
| **C5** Aftertaste | **1.5 min** | Junction/Strip landmark only | **None** — exit read, no new zone |
| **Glue** | **~1.5 min** | Path ribbons + 1–2 doglegs in-ward | No load gates / no fast-travel |

**Soft total ≈ 21 min** → tune to **~20** by trimming Arena_B dwell or S5 roof.

### Fight beat intent (space only)
- **Arena_A (S2):** ~18×12 m · cover blocks 1.0–1.2 m · one mantle at Y≈1.2 · spawn ring clear of MissionVolume pad
- **Arena_B (S3):** ~28×20 m · open center · low planters on rim · keep gate north clear
- **Arena_C (S4):** ~40×12 m corridor · 3–4 waist ribs · side bay 10×8 for Surge utility · line of sight to door frame
- **Arena_D (S5):** hall + N/S room floors · no solid room fills · choke at door · roof pad optional

## Graybox expand (this drop)
1. Navmesh **proxy floors** (`Nav_*`) along spine + each Arena_* (Eng bakes)
2. Place **Arena_A–D** volumes + cover
3. Path **doglegs** so S0→S5 isn’t pad-to-pad teleport feel
4. MissionVolume / MissionNodeId **unchanged**

## Cuts
No skyline · no second ward · no patrol sim required · no crowd · max enterables = hideout (+ optional bodega)

## Success
Demo clears S0→S5 in **15–25 min** without fast-travel; Eng can bake nav on `Nav_*` and hook encounters on `Arena_*`.
