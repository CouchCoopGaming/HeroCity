using UnityEngine;
using HeroCity.Player;
using HeroCity.Mission;
using HeroCity.Surge;
using HeroCity.Narrative;

namespace HeroCity.Level
{
    /// <summary>
    /// Riverward graybox v0 — 400×280 m SW origin (+X east, +Z north).
    /// Pass 3: ferry/S0, alley connectors, hideout interior staging. Matches Riverward-graybox-v0.
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
            (MissionNodeId.S5_Hideout, "S5_Hideout", new Vector3(293f, 0f, 240f), new Vector3(4f, 1f, 4f)),
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

            // C5 aftertaste pad (landmark only — no MissionNodeId yet)
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
                (new Vector3(200f, 0f, 233f), new Vector3(293f, 0f, 240f)),
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
            // Lead S4 → west door face (~X=240), not deep interior
            Box("Approach_Chevron_1", new Vector3(212f, 0.1f, 234f), new Vector3(4f, 0.1f, 2f),
                new Color(0.75f, 0.3f, 0.2f), path);
            Box("Approach_Chevron_2", new Vector3(224f, 0.1f, 237f), new Vector3(4f, 0.1f, 2f),
                new Color(0.75f, 0.3f, 0.2f), path);
            Box("Approach_Chevron_3", new Vector3(235f, 0.1f, 239f), new Vector3(4f, 0.1f, 2f),
                new Color(0.75f, 0.3f, 0.2f), path);
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
                float markH = n.id == MissionNodeId.S5_Hideout ? 5f : 4f;
                Box(n.name + "_Landmark", n.pos + new Vector3(0f, markH * 0.5f, 0f),
                    new Vector3(1.5f, markH, 1.5f), ColorFor(n.id) * 0.75f, sockets);

                var vol = new GameObject(n.name + "_Volume");
                vol.transform.SetParent(sockets, false);
                vol.transform.position = n.pos + Vector3.up * 1.5f;
                var box = vol.AddComponent<BoxCollider>();
                box.isTrigger = true;
                float vx = Mathf.Max(n.size.x * 0.95f, 6f);
                float vz = Mathf.Max(n.size.z * 0.95f, 6f);
                box.size = new Vector3(vx, 5f, vz);
                vol.AddComponent<MissionVolume>().Configure(n.id);
            }
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
            float doorHalf = 2.2f;
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
            Box("Hideout_DoorFrame_L", new Vector3(westX - 0.2f, 2f, cz - doorHalf - 0.4f),
                new Vector3(0.5f, 4f, 0.5f), doorRead, hide);
            Box("Hideout_DoorFrame_R", new Vector3(westX - 0.2f, 2f, cz + doorHalf + 0.4f),
                new Vector3(0.5f, 4f, 0.5f), doorRead, hide);
            Box("Hideout_DoorFrame_Top", new Vector3(westX - 0.2f, 4.6f, cz),
                new Vector3(0.5f, 0.5f, doorHalf * 2f + 1.2f), doorRead, hide);
            Box("Hideout_DoorBeacon", new Vector3(westX - 1.5f, 7.5f, cz),
                new Vector3(1.2f, 3f, 1.2f), doorRead, hide);

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
        }
    }
}
