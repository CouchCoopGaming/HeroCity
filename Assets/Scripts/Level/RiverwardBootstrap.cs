using UnityEngine;
using HeroCity.Player;
using HeroCity.Mission;
using HeroCity.Surge;
using HeroCity.Narrative;
using HeroCity.Combat;

namespace HeroCity.Level
{
    /// <summary>
    /// Riverward graybox v0 — 400×280 m SW origin (+X east, +Z north).
    /// Pass 4 / 20min: Arena_A–D + Nav_* envelopes + path doglegs. MissionVolumes intact. Matches Riverward-20min-pacing-v0.
    /// </summary>
    public class RiverwardBootstrap : MonoBehaviour
    {
        Transform _root;

        static readonly (MissionNodeId id, string name, Vector3 pos, Vector3 size)[] Nodes =
        {
            (MissionNodeId.S0_Boardwalk, "S0_Boardwalk", new Vector3(200f, 0f, 60f), new Vector3(12f, 1f, 12f)),
            (MissionNodeId.S1_Bodega, "S1_Bodega", new Vector3(107f, 0f, 113f), new Vector3(10f, 1f, 10f)),
            (MissionNodeId.S2_AlleyRoof, "S2_AlleyRoof", new Vector3(307f, 0f, 127f), new Vector3(10f, 1f, 10f)),
            (MissionNodeId.S3_Junction, "S3_Junction", new Vector3(200f, 0f, 180f), new Vector3(16f, 1f, 16f)),
            (MissionNodeId.S4_WarehouseApproach, "S4_WarehouseApproach", new Vector3(200f, 0f, 233f), new Vector3(12f, 1f, 12f)),
            (MissionNodeId.S5_Hideout, "S5_Hideout", new Vector3(246f, 0f, 240f), new Vector3(14f, 1f, 12f)), // door threshold apron (was 4x4@293 missable)
        };

        void Awake()
        {
            _root = new GameObject("Riverward_Graybox").transform;
            BuildEnvelope();
            BuildMassing();
            BuildSpinePath();
            BuildNodes();
            BuildHideoutShell();
            BuildBlackoutProps();
            BuildPass3Volumes();
            Build20MinPack();
            SpawnPlayer();
            EnsureSystems();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void BuildEnvelope()
        {
            // Ground slab under playable envelope
            Box("Riverward_Ground", new Vector3(200f, -0.5f, 140f), new Vector3(400f, 1f, 280f),
                new Color(0.22f, 0.24f, 0.27f));

            // Water strip Z[0,40]
            Box("Water", new Vector3(200f, -1.2f, 20f), new Vector3(400f, 0.4f, 40f),
                new Color(0.15f, 0.35f, 0.5f));

            // Boardwalk Z[40,80]
            Box("Boardwalk", new Vector3(200f, 0.05f, 60f), new Vector3(400f, 0.1f, 40f),
                new Color(0.45f, 0.38f, 0.28f));

            // N–S avenues X≈67, 200, 330
            foreach (var x in new[] { 67f, 200f, 330f })
                Box($"Avenue_X{x:0}", new Vector3(x, 0.02f, 140f), new Vector3(10f, 0.04f, 280f),
                    new Color(0.18f, 0.18f, 0.2f));

            // E–W streets Z≈80, 160, 200
            foreach (var z in new[] { 80f, 160f, 200f })
                Box($"Street_Z{z:0}", new Vector3(200f, 0.03f, z), new Vector3(400f, 0.04f, 8f),
                    new Color(0.18f, 0.18f, 0.2f));

            // Pier landmark L1
            Box("Pier_L1", new Vector3(360f, 1.5f, 50f), new Vector3(40f, 3f, 10f),
                new Color(0.35f, 0.32f, 0.28f));
        }

        void BuildMassing()
        {
            var mass = new GameObject("Massing").transform;
            mass.SetParent(_root, false);

            // Two-flat rows Z[90,150] with alley gaps at X≈93, 200, 307
            var twoFlatColor = new Color(0.42f, 0.4f, 0.38f);
            // Flank blocks leaving alley gaps near X≈93, 200, 307
            Box("TwoFlat_W", new Vector3(45f, 4f, 120f), new Vector3(50f, 8f, 55f), twoFlatColor, mass);
            Box("TwoFlat_MidW", new Vector3(145f, 4f, 115f), new Vector3(40f, 7f, 45f), twoFlatColor, mass);
            Box("TwoFlat_MidE", new Vector3(250f, 4f, 125f), new Vector3(45f, 8f, 50f), twoFlatColor, mass);
            Box("TwoFlat_E", new Vector3(370f, 4f, 120f), new Vector3(40f, 8f, 50f), twoFlatColor, mass);

            // Bodega stub S1
            Box("Bodega_S1", new Vector3(107f, 2f, 113f), new Vector3(8f, 4f, 8f),
                new Color(0.7f, 0.5f, 0.25f), mass);
            Box("Bodega_Stoop", new Vector3(107f, 0.5f, 108f), new Vector3(3f, 1f, 2f),
                new Color(0.55f, 0.45f, 0.35f), mass);

            // Local roof S2 — top at Y≈6.5
            Box("AlleyWall_S2_A", new Vector3(300f, 3.5f, 127f), new Vector3(2f, 7f, 18f),
                new Color(0.4f, 0.4f, 0.42f), mass);
            Box("AlleyWall_S2_B", new Vector3(314f, 3.5f, 127f), new Vector3(2f, 7f, 18f),
                new Color(0.4f, 0.4f, 0.42f), mass);
            Box("Roof_S2", new Vector3(307f, 6.5f, 127f), new Vector3(12f, 0.4f, 10f),
                new Color(0.5f, 0.48f, 0.45f), mass);
            // S2 stair/mantle stubs (Systems numbers TBD — Level volume only)
            Box("Stair_S2_Low", new Vector3(307f, 1.1f, 118f), new Vector3(3f, 2.2f, 1.2f),
                new Color(0.48f, 0.46f, 0.42f), mass);
            Box("Stair_S2_Mid", new Vector3(307f, 3.2f, 121f), new Vector3(3f, 2.2f, 1.2f),
                new Color(0.48f, 0.46f, 0.42f), mass);
            Box("Mantle_S2", new Vector3(307f, 5.4f, 124f), new Vector3(3.5f, 0.5f, 1.5f),
                new Color(0.55f, 0.5f, 0.4f), mass);

            // Junction plaza X[120,280] Z[160,200]
            Box("Junction_Plaza", new Vector3(200f, 0.04f, 180f), new Vector3(160f, 0.08f, 40f),
                new Color(0.32f, 0.3f, 0.3f), mass);
            // Precinct stoop — civic edge west of plaza
            Box("Precinct_Stoop", new Vector3(130f, 1.5f, 175f), new Vector3(10f, 3f, 6f),
                new Color(0.5f, 0.45f, 0.55f), mass);
            // North arch/gate toward warehouses
            Box("Junction_Gate_L", new Vector3(190f, 3f, 200f), new Vector3(4f, 6f, 2f),
                new Color(0.45f, 0.4f, 0.4f), mass);
            Box("Junction_Gate_R", new Vector3(210f, 3f, 200f), new Vector3(4f, 6f, 2f),
                new Color(0.45f, 0.4f, 0.4f), mass);
            Box("Junction_Gate_Lint", new Vector3(200f, 5.5f, 200f), new Vector3(24f, 1.5f, 2f),
                new Color(0.4f, 0.38f, 0.38f), mass);

            // Warehouse blocks Z[210,270]
            var wh = new Color(0.38f, 0.32f, 0.28f);
            Box("Warehouse_W", new Vector3(100f, 6f, 240f), new Vector3(60f, 12f, 40f), wh, mass);
            Box("Warehouse_C", new Vector3(180f, 5f, 250f), new Vector3(50f, 10f, 35f), wh, mass);
            Box("Warehouse_E_Approach", new Vector3(220f, 4f, 225f), new Vector3(30f, 8f, 25f), wh, mass);
            Box("Warehouse_NE", new Vector3(255f, 5.5f, 255f), new Vector3(28f, 11f, 30f), wh, mass);
            Box("Warehouse_SE_Flank", new Vector3(235f, 4.5f, 215f), new Vector3(22f, 9f, 18f), wh, mass);
            // Keep sight corridor S4 (200,233) → S5 door (240,240) clear of solids
            Box("Warehouse_LaneGuide_N", new Vector3(200f, 3f, 248f), new Vector3(8f, 6f, 12f), wh, mass);
            Box("Warehouse_LaneGuide_S", new Vector3(200f, 3f, 220f), new Vector3(8f, 6f, 10f), wh, mass);

            // Elevator landmark L3 silhouette
            Box("Elevator_L3", new Vector3(347f, 20f, 250f), new Vector3(8f, 40f, 8f),
                new Color(0.55f, 0.5f, 0.35f), mass);

            // Legacy C5 landmark at Junction (active C5 volume is at door approach)
            Box("C5_Exit_Pad", new Vector3(200f, 0.08f, 200f), new Vector3(8f, 0.1f, 8f),
                new Color(0.6f, 0.55f, 0.7f), mass);
        }


        void BuildSpinePath()
        {
            var path = new GameObject("Spine_Path").transform;
            path.SetParent(_root, false);
            var ribbon = new Color(0.55f, 0.48f, 0.22f);
            // Segment midpoints S0→S1→S2→S3→S4→S5 (flat ribbons for landmark walk)
            (Vector3 a, Vector3 b)[] segs =
            {
                (new Vector3(200f, 0f, 60f), new Vector3(107f, 0f, 113f)),
                (new Vector3(107f, 0f, 113f), new Vector3(307f, 0f, 127f)),
                (new Vector3(307f, 0f, 127f), new Vector3(200f, 0f, 180f)),
                (new Vector3(200f, 0f, 180f), new Vector3(200f, 0f, 233f)),
                (new Vector3(200f, 0f, 233f), new Vector3(246f, 0f, 240f)),
            };
            for (int i = 0; i < segs.Length; i++)
            {
                var a = segs[i].a;
                var b = segs[i].b;
                var mid = (a + b) * 0.5f + Vector3.up * 0.07f;
                var delta = b - a;
                float len = new Vector2(delta.x, delta.z).magnitude;
                var go = Box($"Path_S{i}_S{i + 1}", mid, new Vector3(3.2f, 0.08f, Mathf.Max(len, 1f)), ribbon, path);
                float yaw = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
                go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            }
            // S4→door sightline chevrons (keep east of X=200 approach clear)
            // Lead S4 → west door face (~X=240) — denser chevrons + apron
            Box("Approach_Chevron_1", new Vector3(208f, 0.1f, 234f), new Vector3(5f, 0.12f, 2.5f),
                new Color(0.75f, 0.3f, 0.2f), path);
            Box("Approach_Chevron_2", new Vector3(218f, 0.1f, 236f), new Vector3(5f, 0.12f, 2.5f),
                new Color(0.75f, 0.3f, 0.2f), path);
            Box("Approach_Chevron_3", new Vector3(228f, 0.1f, 238f), new Vector3(5f, 0.12f, 2.5f),
                new Color(0.75f, 0.3f, 0.2f), path);
            Box("Approach_Chevron_4", new Vector3(238f, 0.1f, 240f), new Vector3(5f, 0.12f, 2.5f),
                new Color(0.85f, 0.25f, 0.15f), path);
            Box("Approach_Apron", new Vector3(246f, 0.08f, 240f), new Vector3(16f, 0.12f, 14f),
                new Color(0.7f, 0.28f, 0.18f), path);
        }

        void BuildNodes()
        {
            var sockets = new GameObject("Sockets").transform;
            sockets.SetParent(_root, false);

            foreach (var n in Nodes)
            {
                Box(n.name + "_Pad", n.pos + Vector3.up * 0.06f,
                    new Vector3(n.size.x, 0.12f, n.size.z), ColorFor(n.id), sockets);

                // Readable landmark pole (S5 uses door-scale pad — taller mark)
                float markH = n.id == MissionNodeId.S5_Hideout ? 8f : 4f;
                Box(n.name + "_Landmark", n.pos + new Vector3(0f, markH * 0.5f, 0f),
                    new Vector3(1.5f, markH, 1.5f), ColorFor(n.id) * 0.75f, sockets);

                // S5: do NOT put MissionVolume on the interior/apron pad alone —
                // door volume is built separately at the west door gap.
                if (n.id == MissionNodeId.S5_Hideout)
                    continue;

                var vol = new GameObject(n.name + "_Volume");
                vol.transform.SetParent(sockets, false);
                vol.transform.position = n.pos + Vector3.up * 1.5f;
                var box = vol.AddComponent<BoxCollider>();
                box.isTrigger = true;
                float vx = Mathf.Max(n.size.x * 0.95f, 6f);
                float vz = Mathf.Max(n.size.z * 0.95f, 6f);
                float vy = 5f;
                box.size = new Vector3(vx, vy, vz);
                vol.AddComponent<MissionVolume>().Configure(n.id);
            }

            BuildHideoutDoorAndC5(sockets);
        }

        /// <summary>S5 door volume + solid blocker (DoorUnlocked) + C5 Aftertaste pad.</summary>
        void BuildHideoutDoorAndC5(Transform sockets)
        {
            // West door gap ~X=240, Z=240 — MissionVolume requires DoorUnlocked
            var doorVol = new GameObject("Hideout_Door_Volume");
            doorVol.transform.SetParent(sockets, false);
            doorVol.transform.position = new Vector3(240f, 1.5f, 240f);
            var doorBox = doorVol.AddComponent<BoxCollider>();
            doorBox.isTrigger = true;
            doorBox.size = new Vector3(4f, 5f, 5f);
            doorVol.AddComponent<MissionVolume>().Configure(MissionNodeId.S5_Hideout, requireDoor: true);

            // Physical door blocker filling the gap (solid collider, red-tinted)
            var blocker = Box("Hideout_Door_Blocker", new Vector3(240f, 2.5f, 240f),
                new Vector3(1.4f, 5f, 7.5f), new Color(0.85f, 0.2f, 0.15f, 1f), sockets);
            // Ensure solid (non-trigger) collider remains
            var bc = blocker.GetComponent<BoxCollider>();
            if (bc != null) bc.isTrigger = false;
            blocker.AddComponent<DoorUnlockController>();

            // C5 Aftertaste pad just outside door approach (~250, 0, 240) — no door gate
            Box("C5_Aftertaste_Pad", new Vector3(250f, 0.08f, 240f), new Vector3(8f, 0.12f, 8f),
                new Color(0.6f, 0.55f, 0.7f), sockets);
            Box("C5_Aftertaste_Landmark", new Vector3(250f, 2.5f, 240f), new Vector3(1.2f, 5f, 1.2f),
                new Color(0.55f, 0.45f, 0.7f), sockets);
            var c5 = new GameObject("C5_Aftertaste_Volume");
            c5.transform.SetParent(sockets, false);
            c5.transform.position = new Vector3(250f, 1.5f, 240f);
            var c5box = c5.AddComponent<BoxCollider>();
            c5box.isTrigger = true;
            c5box.size = new Vector3(8f, 5f, 8f);
            c5.AddComponent<MissionVolume>().Configure(MissionNodeId.C5_Aftertaste, requireDoor: false);
        }

        void BuildHideoutShell()
        {
            var hide = new GameObject("Hideout_Shell").transform;
            hide.SetParent(_root, false);

            // Footprint X[240,347] Z[213,267] → center (293.5, 240), size 107×54
            var shell = new Color(0.32f, 0.28f, 0.26f);
            float cx = 293.5f, cz = 240f;
            float halfW = 53.5f, halfD = 27f, wallH = 10f, thick = 1.2f;

            // N / S / E full walls; W wall with door gap at S5 (293, 240) — west face ≈ X=240
            Box("Hideout_Wall_N", new Vector3(cx, wallH * 0.5f, cz + halfD), new Vector3(halfW * 2f, wallH, thick), shell, hide);
            Box("Hideout_Wall_S", new Vector3(cx, wallH * 0.5f, cz - halfD), new Vector3(halfW * 2f, wallH, thick), shell, hide);
            Box("Hideout_Wall_E", new Vector3(cx + halfW, wallH * 0.5f, cz), new Vector3(thick, wallH, halfD * 2f), shell, hide);

            // West wall split around door (door ~4 m at Z=240)
            float doorHalf = 4.0f; // was 2.2 — Eng: 4x4 pad missable, enlarge door read
            float westX = cx - halfW;
            float northSegZ = (cz + halfD + cz + doorHalf) * 0.5f;
            float northSegD = halfD - doorHalf;
            float southSegZ = (cz - halfD + cz - doorHalf) * 0.5f;
            float southSegD = halfD - doorHalf;
            Box("Hideout_Wall_W_N", new Vector3(westX, wallH * 0.5f, northSegZ),
                new Vector3(thick, wallH, northSegD * 2f), shell, hide);
            Box("Hideout_Wall_W_S", new Vector3(westX, wallH * 0.5f, southSegZ),
                new Vector3(thick, wallH, southSegD * 2f), shell, hide);
            // Door lintel + high-contrast frame (readable from S4 ~40 m)
            Box("Hideout_Door_Lintel", new Vector3(westX, 4.2f, cz),
                new Vector3(thick, 1.2f, doorHalf * 2f), shell, hide);
            var doorRead = new Color(0.85f, 0.25f, 0.18f);
            Box("Hideout_DoorFrame_L", new Vector3(westX - 0.3f, 3f, cz - doorHalf - 0.6f),
                new Vector3(0.8f, 6f, 0.8f), doorRead, hide);
            Box("Hideout_DoorFrame_R", new Vector3(westX - 0.3f, 3f, cz + doorHalf + 0.6f),
                new Vector3(0.8f, 6f, 0.8f), doorRead, hide);
            Box("Hideout_DoorFrame_Top", new Vector3(westX - 0.3f, 6.2f, cz),
                new Vector3(0.8f, 0.8f, doorHalf * 2f + 2f), doorRead, hide);
            Box("Hideout_DoorBeacon", new Vector3(westX - 2f, 9f, cz),
                new Vector3(2.2f, 6f, 2.2f), doorRead, hide);
            Box("Hideout_DoorBeacon_Cap", new Vector3(westX - 2f, 12.5f, cz),
                new Vector3(3.5f, 1f, 3.5f), doorRead, hide);

            // Roof deck Y≈10
            Box("Hideout_Roof", new Vector3(cx, 10.2f, cz), new Vector3(halfW * 2f - 1f, 0.4f, halfD * 2f - 1f),
                new Color(0.28f, 0.26f, 0.24f), hide);

            // Interior floor hint
            Box("Hideout_Floor", new Vector3(cx, 0.05f, cz), new Vector3(halfW * 2f - 2f, 0.1f, halfD * 2f - 2f),
                new Color(0.25f, 0.22f, 0.2f), hide);
        }

        void BuildBlackoutProps()
        {
            var props = new GameObject("Blackout_Props").transform;
            props.SetParent(_root, false);

            // Dead windows on hideout west approach face
            for (int i = 0; i < 4; i++)
            {
                float z = 228f + i * 6f;
                Box($"BlackedWindow_{i}", new Vector3(241.2f, 5f, z), new Vector3(0.3f, 2.2f, 2.5f),
                    new Color(0.05f, 0.05f, 0.06f), props);
            }

            // Fused breaker near S5 door
            Box("Prop_Breaker_Blackout", new Vector3(288f, 1.4f, 240f),
                new Vector3(0.6f, 1.6f, 0.4f), new Color(0.15f, 0.12f, 0.1f), props);

            // Dead pier neon echo on L1 + killed neon inside hideout
            Box("DeadNeon_Pier", new Vector3(360f, 4f, 50f), new Vector3(18f, 1.2f, 0.4f),
                new Color(0.12f, 0.1f, 0.15f), props);
            Box("DeadNeon_Hideout", new Vector3(300f, 6f, 240f), new Vector3(14f, 1f, 0.35f),
                new Color(0.1f, 0.08f, 0.12f), props);
        }


        void BuildPass3Volumes()
        {
            var p3 = new GameObject("Pass3_Volumes").transform;
            p3.SetParent(_root, false);

            // Ferry / Call landmark at S0 — boardwalk edge over water
            Box("Ferry_Hull", new Vector3(200f, 1.2f, 38f), new Vector3(28f, 2.4f, 10f),
                new Color(0.25f, 0.4f, 0.55f), p3);
            Box("Ferry_Cabin", new Vector3(200f, 3.2f, 38f), new Vector3(10f, 2f, 6f),
                new Color(0.3f, 0.45f, 0.6f), p3);
            Box("Ferry_Ramp", new Vector3(200f, 0.6f, 48f), new Vector3(6f, 0.4f, 12f),
                new Color(0.4f, 0.35f, 0.28f), p3);

            // Alley connectors — gaps at X≈93, 200, 307 through Two-Flats Z[90,150]
            var alley = new Color(0.28f, 0.28f, 0.3f);
            foreach (var (x, name) in new[] { (93f, "W"), (200f, "C"), (307f, "E") })
            {
                Box($"Alley_{name}_Floor", new Vector3(x, 0.06f, 120f), new Vector3(8f, 0.08f, 60f), alley, p3);
                Box($"Alley_{name}_WallL", new Vector3(x - 4.5f, 3.5f, 120f), new Vector3(1f, 7f, 55f),
                    new Color(0.36f, 0.35f, 0.34f), p3);
                Box($"Alley_{name}_WallR", new Vector3(x + 4.5f, 3.5f, 120f), new Vector3(1f, 7f, 55f),
                    new Color(0.36f, 0.35f, 0.34f), p3);
            }
            // Cross-link S1 (107,113) toward alley W then east toward S2
            Box("AlleyLink_S1_W", new Vector3(100f, 0.07f, 113f), new Vector3(14f, 0.08f, 3f),
                new Color(0.5f, 0.42f, 0.2f), p3);
            Box("AlleyLink_S2_E", new Vector3(307f, 0.07f, 140f), new Vector3(3f, 0.08f, 24f),
                new Color(0.5f, 0.42f, 0.2f), p3);

            // Hideout interior staging — floors + partial walls (not solid room fills)
            var floor = new Color(0.3f, 0.27f, 0.25f);
            var wall = new Color(0.34f, 0.3f, 0.28f);
            Box("Hideout_Hall_Floor", new Vector3(270f, 0.08f, 240f), new Vector3(40f, 0.1f, 8f), floor, p3);
            Box("Hideout_Hall_WallN", new Vector3(270f, 2.5f, 244.5f), new Vector3(40f, 5f, 0.4f), wall, p3);
            Box("Hideout_Hall_WallS", new Vector3(270f, 2.5f, 235.5f), new Vector3(40f, 5f, 0.4f), wall, p3);
            Box("Hideout_Room_N_Floor", new Vector3(310f, 0.08f, 255f), new Vector3(24f, 0.1f, 16f), floor, p3);
            Box("Hideout_Room_S_Floor", new Vector3(310f, 0.08f, 225f), new Vector3(24f, 0.1f, 16f), floor, p3);
            Box("Hideout_Room_N_Wall", new Vector3(310f, 2.5f, 263f), new Vector3(24f, 5f, 0.4f), wall, p3);
            Box("Hideout_Room_S_Wall", new Vector3(310f, 2.5f, 217f), new Vector3(24f, 5f, 0.4f), wall, p3);
            // Roof access stub (step boxes — Systems climb TBD)
            Box("Hideout_RoofStair_1", new Vector3(330f, 1.5f, 250f), new Vector3(3f, 3f, 3f),
                new Color(0.45f, 0.4f, 0.35f), p3);
            Box("Hideout_RoofStair_2", new Vector3(330f, 4.5f, 253f), new Vector3(3f, 3f, 3f),
                new Color(0.45f, 0.4f, 0.35f), p3);
            Box("Hideout_RoofStair_3", new Vector3(330f, 7.5f, 256f), new Vector3(3f, 3f, 3f),
                new Color(0.45f, 0.4f, 0.35f), p3);
            Box("Hideout_RoofDeck_Pad", new Vector3(320f, 10.4f, 250f), new Vector3(16f, 0.2f, 12f),
                new Color(0.55f, 0.2f, 0.18f), p3);

            // Bodega optional tiny shell — open stoop volume, not a solid fill
            Box("Bodega_Floor", new Vector3(107f, 0.1f, 113f), new Vector3(5f, 0.1f, 5f),
                new Color(0.55f, 0.4f, 0.2f), p3);
        }


        /// <summary>~20 min SP demo pack — arenas + nav proxy floors + path doglegs. No new districts.</summary>
        void Build20MinPack()
        {
            var root = new GameObject("Pass4_20min").transform;
            root.SetParent(_root, false);
            var navC = new Color(0.2f, 0.45f, 0.28f);
            var arenaFloor = new Color(0.55f, 0.22f, 0.18f);
            var cover = new Color(0.4f, 0.38f, 0.35f);

            // --- Nav proxy floors along spine (Eng bakes) ---
            Box("Nav_Spine_S0_S1", new Vector3(150f, 0.02f, 85f), new Vector3(100f, 0.04f, 14f), navC, root);
            Box("Nav_Spine_S1_S2", new Vector3(200f, 0.02f, 120f), new Vector3(220f, 0.04f, 12f), navC, root);
            Box("Nav_Spine_S2_S3", new Vector3(250f, 0.02f, 155f), new Vector3(120f, 0.04f, 14f), navC, root);
            Box("Nav_Spine_S3_S4", new Vector3(200f, 0.02f, 205f), new Vector3(16f, 0.04f, 55f), navC, root);
            Box("Nav_Spine_S4_S5", new Vector3(245f, 0.02f, 236f), new Vector3(95f, 0.04f, 14f), navC, root);
            Box("Nav_Boardwalk", new Vector3(200f, 0.02f, 60f), new Vector3(80f, 0.04f, 30f), navC, root);
            Box("Nav_Junction", new Vector3(200f, 0.02f, 180f), new Vector3(100f, 0.04f, 36f), navC, root);
            Box("Nav_Hideout_Interior", new Vector3(293f, 0.02f, 240f), new Vector3(90f, 0.04f, 45f), navC, root);

            // --- Arena_A @ S2 alley (~18×12) ---
            var aA = new GameObject("Arena_A").transform;
            aA.SetParent(root, false);
            Box("Arena_A_Floor", new Vector3(307f, 0.05f, 140f), new Vector3(18f, 0.1f, 12f), arenaFloor, aA);
            Box("Arena_A_Cover_1", new Vector3(301f, 0.6f, 138f), new Vector3(2.5f, 1.2f, 1.2f), cover, aA);
            Box("Arena_A_Cover_2", new Vector3(313f, 0.6f, 142f), new Vector3(2.5f, 1.2f, 1.2f), cover, aA);
            Box("Arena_A_Cover_3", new Vector3(307f, 0.6f, 145f), new Vector3(3f, 1.1f, 1.2f), cover, aA);
            Box("Arena_A_Mantle", new Vector3(307f, 1.2f, 134f), new Vector3(4f, 0.4f, 1.2f), cover, aA);
            Box("Nav_Arena_A", new Vector3(307f, 0.02f, 140f), new Vector3(20f, 0.04f, 14f), navC, aA);

            // --- Arena_B @ Junction (~28×20) west of gate ---
            var aB = new GameObject("Arena_B").transform;
            aB.SetParent(root, false);
            Box("Arena_B_Floor", new Vector3(160f, 0.05f, 178f), new Vector3(28f, 0.1f, 20f), arenaFloor, aB);
            Box("Arena_B_Rim_1", new Vector3(148f, 0.5f, 170f), new Vector3(3f, 1f, 2f), cover, aB);
            Box("Arena_B_Rim_2", new Vector3(172f, 0.5f, 186f), new Vector3(3f, 1f, 2f), cover, aB);
            Box("Arena_B_Rim_3", new Vector3(155f, 0.5f, 188f), new Vector3(4f, 1f, 2f), cover, aB);
            Box("Nav_Arena_B", new Vector3(160f, 0.02f, 178f), new Vector3(30f, 0.04f, 22f), navC, aB);

            // --- Arena_C linear S4→door (~40×12) ---
            var aC = new GameObject("Arena_C").transform;
            aC.SetParent(root, false);
            Box("Arena_C_Floor", new Vector3(220f, 0.05f, 236f), new Vector3(40f, 0.1f, 12f), arenaFloor, aC);
            Box("Arena_C_Rib_1", new Vector3(210f, 0.6f, 231f), new Vector3(2f, 1.2f, 1.5f), cover, aC);
            Box("Arena_C_Rib_2", new Vector3(220f, 0.6f, 241f), new Vector3(2f, 1.2f, 1.5f), cover, aC);
            Box("Arena_C_Rib_3", new Vector3(230f, 0.6f, 231f), new Vector3(2f, 1.2f, 1.5f), cover, aC);
            Box("Arena_C_Rib_4", new Vector3(235f, 0.6f, 241f), new Vector3(2f, 1.2f, 1.5f), cover, aC);
            Box("Arena_C_SideBay", new Vector3(218f, 0.05f, 248f), new Vector3(10f, 0.1f, 8f), arenaFloor, aC);
            Box("Nav_Arena_C", new Vector3(220f, 0.02f, 238f), new Vector3(44f, 0.04f, 22f), navC, aC);

            // --- Arena_D hideout interior ---
            var aD = new GameObject("Arena_D").transform;
            aD.SetParent(root, false);
            Box("Arena_D_HallMark", new Vector3(270f, 0.06f, 240f), new Vector3(36f, 0.08f, 6f), arenaFloor, aD);
            Box("Arena_D_RoomN_Mark", new Vector3(310f, 0.06f, 255f), new Vector3(20f, 0.08f, 12f), arenaFloor, aD);
            Box("Arena_D_RoomS_Mark", new Vector3(310f, 0.06f, 225f), new Vector3(20f, 0.08f, 12f), arenaFloor, aD);
            Box("Arena_D_Cover_Crate", new Vector3(280f, 0.7f, 243f), new Vector3(2f, 1.4f, 2f), cover, aD);
            Box("Arena_D_Cover_Crate2", new Vector3(300f, 0.7f, 237f), new Vector3(2f, 1.4f, 2f), cover, aD);
            Box("Nav_Arena_D", new Vector3(290f, 0.02f, 240f), new Vector3(70f, 0.04f, 40f), navC, aD);

            // --- Path doglegs (anti-teleport stubs) still in-ward ---
            var dog = new Color(0.55f, 0.48f, 0.22f);
            Box("Dogleg_S0_S1", new Vector3(160f, 0.08f, 70f), new Vector3(3f, 0.08f, 28f), dog, root);
            Box("Dogleg_S1_S2_N", new Vector3(200f, 0.08f, 130f), new Vector3(3f, 0.08f, 20f), dog, root);
            Box("Dogleg_S2_S3_W", new Vector3(260f, 0.08f, 160f), new Vector3(40f, 0.08f, 3f), dog, root);
            Box("Dogleg_S4_bay", new Vector3(205f, 0.08f, 245f), new Vector3(3f, 0.08f, 16f), dog, root);
        }

        GameObject Box(string name, Vector3 pos, Vector3 scale, Color color, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent != null ? parent : _root, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material.color = color;
            return go;
        }

        Color ColorFor(MissionNodeId id) => id switch
        {
            MissionNodeId.S0_Boardwalk => new Color(0.3f, 0.55f, 0.75f),
            MissionNodeId.S1_Bodega => new Color(0.7f, 0.5f, 0.25f),
            MissionNodeId.S2_AlleyRoof => new Color(0.45f, 0.45f, 0.5f),
            MissionNodeId.S3_Junction => new Color(0.55f, 0.4f, 0.55f),
            MissionNodeId.S4_WarehouseApproach => new Color(0.5f, 0.35f, 0.3f),
            MissionNodeId.C5_Aftertaste => new Color(0.6f, 0.55f, 0.7f),
            _ => new Color(0.75f, 0.25f, 0.2f)
        };

        void SpawnPlayer()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Player";
            go.tag = "Player";
            Object.Destroy(go.GetComponent<Collider>());
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0, 0.9f, 0);
            go.transform.position = Nodes[0].pos + new Vector3(0f, 1f, -6f);
            go.AddComponent<ThirdPersonMotor>();
            go.AddComponent<SurgeController>();
            go.AddComponent<PlayerHealth>();
            go.AddComponent<PlayerCombat>();

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            var tpc = camGo.AddComponent<ThirdPersonCamera>();
            tpc.SetTarget(go.transform);
        }

        void EnsureSystems()
        {
            if (FindFirstObjectByType<MissionChainController>() == null)
                new GameObject("MissionChain").AddComponent<MissionChainController>();
            if (FindFirstObjectByType<NemesisIntroHook>() == null)
                new GameObject("NemesisIntro").AddComponent<NemesisIntroHook>();
            if (FindFirstObjectByType<EncounterDirector>() == null)
                new GameObject("Encounters").AddComponent<EncounterDirector>();
            if (FindFirstObjectByType<NemesisFight>() == null)
                new GameObject("NemesisFight").AddComponent<NemesisFight>();
            if (FindFirstObjectByType<ObjectiveHud>() == null)
                new GameObject("ObjectiveHud").AddComponent<ObjectiveHud>();

            var player = GameObject.Find("Player");
            if (player != null)
            {
                EncounterDirector.Instance?.SetPlayer(player.transform);
                // Kick S0 wave after short beat
                MissionChainController.Instance?.TryAdvance(MissionNodeId.S0_Boardwalk);
            }
        }
    }
}
