using System.Collections.Generic;
using UnityEngine;
using HeroCity.Mission;

namespace HeroCity.Combat
{
    /// <summary>Gates mission advance until wave cleared. Spawns at Arena_A–D. HP: Trash 80 / Elite 320.</summary>
    public class EncounterDirector : MonoBehaviour
    {
        public static EncounterDirector Instance { get; private set; }

        public const float TrashHp = 80f;
        public const float EliteHp = 320f;
        public const float EliteArmor = 40f;

        readonly List<Hostile> _alive = new List<Hostile>();
        MissionNodeId _activeNode = MissionNodeId.S0_Boardwalk;
        bool _waveActive;
        string _status = "Reach C1 Call (S0)";
        Transform _player;

        // Wave packs ~3–5 trash, 0–2 elite/room (Systems P0)
        static readonly Dictionary<MissionNodeId, (int trash, int elite)> Waves = new()
        {
            { MissionNodeId.S0_Boardwalk, (3, 0) },
            { MissionNodeId.S1_Bodega, (4, 0) },
            { MissionNodeId.S2_AlleyRoof, (4, 1) },
            { MissionNodeId.S3_Junction, (5, 1) },
            { MissionNodeId.S4_WarehouseApproach, (5, 2) },
            { MissionNodeId.S5_Hideout, (4, 1) }, // prelude before Blackout
        };

        public bool WaveClear => !_waveActive || _alive.Count == 0;
        public bool WaveActive => _waveActive;
        public MissionNodeId ActiveNode => _activeNode;
        public string Status => _status;

        void Awake() => Instance = this;

        public void SetPlayer(Transform player) => _player = player;

        public void OnEnteredNode(MissionNodeId node)
        {
            if (node == MissionNodeId.C5_Aftertaste) return;
            if (_waveActive && !WaveClear) return;
            if ((int)node < (int)_activeNode) return;

            _activeNode = node;
            StartWave(node);
        }

        /// <summary>Soft-checkpoint respawn: restart wave at Current if it was active.</summary>
        public void RestartWaveForCurrent()
        {
            ClearAliveHostiles();
            StartWave(_activeNode);
        }

        void StartWave(MissionNodeId node)
        {
            ClearDead();
            if (!Waves.TryGetValue(node, out var w)) w = (3, 0);
            _waveActive = true;
            _status = $"Clear hostiles at {node} ({w.trash}+{w.elite} elite)";
            Vector3 center = SpawnCenter(node);
            for (int i = 0; i < w.trash; i++)
                Spawn(center, TrashHp, false);
            for (int i = 0; i < w.elite; i++)
                Spawn(center, EliteHp, true);
            Debug.Log($"[Encounter] Wave {node} trash={w.trash} elite={w.elite} @ {center}");
        }

        /// <summary>S2→Arena_A · S3→Arena_B · S4→Arena_C · S5→Arena_D · S0/S1 pads.</summary>
        public static Vector3 SpawnCenter(MissionNodeId id) => id switch
        {
            MissionNodeId.S0_Boardwalk => new Vector3(200f, 1f, 60f),
            MissionNodeId.S1_Bodega => new Vector3(107f, 1f, 113f),
            MissionNodeId.S2_AlleyRoof => new Vector3(307f, 1f, 140f), // Arena_A
            MissionNodeId.S3_Junction => new Vector3(160f, 1f, 178f),  // Arena_B
            MissionNodeId.S4_WarehouseApproach => new Vector3(220f, 1f, 236f), // Arena_C
            MissionNodeId.S5_Hideout => new Vector3(280f, 1f, 240f), // Arena_D interior
            _ => new Vector3(293f, 1f, 240f)
        };

        void Spawn(Vector3 center, float hp, bool elite)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = elite ? "Elite" : "Trash";
            // Remove mesh collider; Hostile.Awake adds non-trigger CapsuleCollider for LMB hits
            Object.Destroy(go.GetComponent<Collider>());
            float ang = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float rad = Random.Range(3.5f, 7f);
            go.transform.position = center + new Vector3(Mathf.Cos(ang) * rad, 0f, Mathf.Sin(ang) * rad);
            var h = go.AddComponent<Hostile>();
            h.Configure(hp, elite, _player, elite ? EliteArmor : 0f);
            _alive.Add(h);
        }

        public void OnHostileDied(Hostile h)
        {
            _alive.Remove(h);
            ClearDead();
            if (_waveActive && _alive.Count == 0)
            {
                _waveActive = false;
                _status = _activeNode == MissionNodeId.S5_Hideout
                    ? "Wave clear — face Blackout (VO.N1.Reveal)"
                    : "Clear — advance toward next beat";
                MissionChainController.Instance?.NotifyWaveCleared(_activeNode);
                if (_activeNode == MissionNodeId.S5_Hideout)
                {
                    FindFirstObjectByType<HeroCity.Narrative.NemesisIntroHook>()?.BeginIntro();
                    FindFirstObjectByType<NemesisFight>()?.Begin();
                }
            }
            else
                _status = $"Hostiles left: {_alive.Count}";
        }

        void ClearAliveHostiles()
        {
            foreach (var h in _alive.ToArray())
            {
                if (h != null) h.ForceDespawn();
            }
            _alive.Clear();
            _waveActive = false;
        }

        void ClearDead()
        {
            _alive.RemoveAll(h => h == null || !h.Alive);
        }

        void Update()
        {
            ClearDead();
            if (_waveActive && Input.GetKeyDown(KeyCode.K))
            {
                foreach (var h in _alive.ToArray())
                    if (h != null) h.TakeDamage(9999f, true);
            }
        }
    }
}
