using UnityEngine;
using HeroCity.Player;
using HeroCity.Mission;
using HeroCity.Surge;
using HeroCity.Narrative;

namespace HeroCity.Level
{
    /// <summary>
    /// Runtime graybox S0→S5 along Riverward spine (~120s walk). Level meters TBD — placeholders.
    /// </summary>
    public class RiverwardBootstrap : MonoBehaviour
    {
        static readonly (MissionNodeId id, string name, Vector3 pos, Vector3 size)[] Nodes =
        {
            (MissionNodeId.S0_Boardwalk, "S0_Boardwalk", new Vector3(0, 0, 0), new Vector3(18, 1, 12)),
            (MissionNodeId.S1_Bodega, "S1_Bodega", new Vector3(0, 0, 28), new Vector3(14, 1, 12)),
            (MissionNodeId.S2_AlleyRoof, "S2_AlleyRoof", new Vector3(8, 0, 56), new Vector3(12, 1, 14)),
            (MissionNodeId.S3_Junction, "S3_Junction", new Vector3(0, 0, 86), new Vector3(16, 1, 14)),
            (MissionNodeId.S4_WarehouseApproach, "S4_WarehouseApproach", new Vector3(-6, 0, 118), new Vector3(14, 1, 16)),
            (MissionNodeId.S5_Hideout, "S5_Hideout", new Vector3(-6, 0, 150), new Vector3(20, 1, 18)),
        };

        void Awake()
        {
            BuildGround();
            BuildNodes();
            SpawnPlayer();
            EnsureSystems();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Riverward_Ground";
            ground.transform.position = new Vector3(0f, -0.5f, 80f);
            ground.transform.localScale = new Vector3(60f, 1f, 200f);
            ground.GetComponent<Renderer>().material.color = new Color(0.25f, 0.28f, 0.32f);
        }

        void BuildNodes()
        {
            foreach (var n in Nodes)
            {
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = n.name + "_Pad";
                floor.transform.position = n.pos + Vector3.up * 0.05f;
                floor.transform.localScale = new Vector3(n.size.x, 0.1f, n.size.z);
                floor.GetComponent<Renderer>().material.color = ColorFor(n.id);

                // Landmark stub
                var mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mark.name = n.name + "_Landmark";
                mark.transform.position = n.pos + new Vector3(0f, 2f, 0f);
                mark.transform.localScale = new Vector3(2f, 4f, 2f);
                mark.GetComponent<Renderer>().material.color = ColorFor(n.id) * 0.7f;

                var vol = new GameObject(n.name + "_Volume");
                vol.transform.position = n.pos + Vector3.up;
                var box = vol.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = new Vector3(n.size.x * 0.9f, 4f, n.size.z * 0.9f);
                var mv = vol.AddComponent<MissionVolume>();
                mv.Configure(n.id);
            }

            // Hideout shell walls at S5
            var s5 = Nodes[5].pos;
            MakeWall("Hideout_Wall_N", s5 + new Vector3(0, 2, 10), new Vector3(22, 4, 1));
            MakeWall("Hideout_Wall_S", s5 + new Vector3(0, 2, -10), new Vector3(22, 4, 1));
            MakeWall("Hideout_Wall_E", s5 + new Vector3(11, 2, 0), new Vector3(1, 4, 20));
            MakeWall("Hideout_Roof", s5 + new Vector3(0, 4.5f, 0), new Vector3(20, 0.4f, 18));
        }

        void MakeWall(string name, Vector3 pos, Vector3 scale)
        {
            var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
            w.name = name;
            w.transform.position = pos;
            w.transform.localScale = scale;
            w.GetComponent<Renderer>().material.color = new Color(0.35f, 0.3f, 0.28f);
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
            cc.height = 1.8f; cc.radius = 0.35f; cc.center = new Vector3(0, 0.9f, 0);
            go.transform.position = Nodes[0].pos + new Vector3(0f, 1f, -4f);
            go.AddComponent<ThirdPersonMotor>();
            go.AddComponent<SurgeController>();

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
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
