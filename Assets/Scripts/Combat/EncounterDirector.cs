using System.Collections.Generic;
using UnityEngine;
using HeroCity.Mission;

namespace HeroCity.Combat
{
    /// <summary>Gates mission advance until wave cleared. Paces ~20 min SP demo.</summary>
    public class EncounterDirector : MonoBehaviour
    {
        public static EncounterDirector Instance { get; private set; }

        readonly List<Hostile> _alive = new List<Hostile>();
        MissionNodeId _activeNode = MissionNodeId.S0_Boardwalk;
        bool _waveActive;
        string _status = "Reach C1 Call (S0)";
        Transform _player;

        // Wave sizes tuned for ~20 min with travel + SURGE learning
        static readonly Dictionary<MissionNodeId, (int trash, int elite)> Waves = new()
        {
            { MissionNodeId.S0_Boardwalk, (3, 0) },
            { MissionNodeId.S1_Bodega, (5, 0) },
            { MissionNodeId.S2_AlleyRoof, (5, 1) },
            { MissionNodeId.S3_Junction, (6, 1) },
            { MissionNodeId.S4_WarehouseApproach, (6, 2) },
            { MissionNodeId.S5_Hideout, (4, 1) }, // prelude before Watcher
        };

        public bool WaveClear => !_waveActive || _alive.Count == 0;
        public string Status => _status;

        void Awake() => Instance = this;

        public void SetPlayer(Transform player) => _player = player;

        public void OnEnteredNode(MissionNodeId node)
        {
            if (_waveActive && !WaveClear) return; // softlock guard: ignore until clear
            if ((int)node < (int)_activeNode) return;

            _activeNode = node;
            StartWave(node);
        }

        void StartWave(MissionNodeId node)
        {
            ClearDead();
            if (!Waves.TryGetValue(node, out var w)) w = (3, 0);
            _waveActive = true;
            _status = $"Clear hostiles at {node} ({w.trash}+{w.elite} elite)";
            Vector3 center = NodePos(node);
            for (int i = 0; i < w.trash; i++)
                Spawn(center, 32f, false);
            for (int i = 0; i < w.elite; i++)
                Spawn(center, 55f, true);
            Debug.Log($"[Encounter] Wave {node} trash={w.trash} elite={w.elite}");
        }

        Vector3 NodePos(MissionNodeId id) => id switch
        {
            MissionNodeId.S0_Boardwalk => new Vector3(200f, 0f, 60f),
            MissionNodeId.S1_Bodega => new Vector3(107f, 0f, 113f),
            MissionNodeId.S2_AlleyRoof => new Vector3(307f, 0f, 127f),
            MissionNodeId.S3_Junction => new Vector3(200f, 0f, 180f),
            MissionNodeId.S4_WarehouseApproach => new Vector3(200f, 0f, 233f),
            _ => new Vector3(293f, 0f, 240f)
        };

        void Spawn(Vector3 center, float hp, bool elite)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = elite ? "Elite" : "Trash";
            Object.Destroy(go.GetComponent<Collider>());
            float ang = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float rad = Random.Range(3.5f, 7f);
            go.transform.position = center + new Vector3(Mathf.Cos(ang) * rad, 1f, Mathf.Sin(ang) * rad);
            var h = go.AddComponent<Hostile>();
            h.Configure(hp, elite, _player);
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
                    ? "Wave clear — face The Watcher"
                    : $"Clear — advance toward next beat";
                MissionChainController.Instance?.NotifyWaveCleared(_activeNode);
                if (_activeNode == MissionNodeId.S5_Hideout)
                    FindFirstObjectByType<NemesisFight>()?.Begin();
            }
            else
                _status = $"Hostiles left: {_alive.Count}";
        }

        void ClearDead()
        {
            _alive.RemoveAll(h => h == null || !h.Alive);
        }

        void Update()
        {
            ClearDead();
            // Softlock escape: hold K  to skip wave (debug)
            if (_waveActive && Input.GetKeyDown(KeyCode.K))
            {
                foreach (var h in _alive.ToArray())
                    if (h != null) h.TakeDamage(9999f, true);
            }
        }
    }
}
