using UnityEngine;
using HeroCity.Combat;
using HeroCity.Narrative;

namespace HeroCity.Mission
{
    /// <summary>S0→S5 chain. Advance on volume enter; wave clear required before next objective unlocks.</summary>
    public class MissionChainController : MonoBehaviour
    {
        public static MissionChainController Instance { get; private set; }
        public MissionNodeId Current { get; private set; } = MissionNodeId.S0_Boardwalk;
        public bool Complete { get; private set; }
        bool _waveClearedForCurrent;

        void Awake() => Instance = this;

        public void TryAdvance(MissionNodeId reached)
        {
            if (Complete) return;
            // Must clear current wave before jumping ahead more than one
            if ((int)reached > (int)Current)
            {
                if (!_waveClearedForCurrent && reached != Current)
                {
                    ObjectiveHud hud = FindFirstObjectByType<ObjectiveHud>();
                    hud?.SetObjective("Clear hostiles before advancing");
                    return;
                }
            }
            if ((int)reached < (int)Current) return;

            if (reached != Current)
            {
                Current = reached;
                _waveClearedForCurrent = false;
                Debug.Log($"[Mission] Advanced → {Current}");
            }

            EncounterDirector.Instance?.OnEnteredNode(Current);
            FindFirstObjectByType<ObjectiveHud>()?.SetObjective(StoryObjective(Current));
        }

        public void NotifyWaveCleared(MissionNodeId node)
        {
            if (node != Current) return;
            _waveClearedForCurrent = true;
            if (Current == MissionNodeId.S5_Hideout)
            {
                // NemesisFight handles intro after prelude wave
                return;
            }
            FindFirstObjectByType<ObjectiveHud>()?.SetObjective($"Advance to {NextLabel(Current)}");
        }

        public void MarkStoryComplete()
        {
            Complete = true;
        }

        static string NextLabel(MissionNodeId id)
        {
            int n = Mathf.Min((int)id + 1, 5);
            return StoryLabel((MissionNodeId)n);
        }

        static string StoryLabel(MissionNodeId id) => id switch
        {
            MissionNodeId.S0_Boardwalk => "C1 Call (S0)",
            MissionNodeId.S1_Bodega => "C2 Pattern (S1)",
            MissionNodeId.S2_AlleyRoof => "C3 Trail (S2)",
            MissionNodeId.S3_Junction => "C3 Funnel (S3)",
            MissionNodeId.S4_WarehouseApproach => "C4 Door (S4)",
            MissionNodeId.S5_Hideout => "N1 Hideout (S5)",
            _ => id.ToString()
        };

        static string StoryObjective(MissionNodeId id) => id switch
        {
            MissionNodeId.S0_Boardwalk => "C1 Call — clear boardwalk trash, learn Arc fire (LMB) + SURGE Q/E/F",
            MissionNodeId.S1_Bodega => "C2 Pattern — clear bodega stoop pack",
            MissionNodeId.S2_AlleyRoof => "C3 Trail — alley/roof hostiles (elite in mix)",
            MissionNodeId.S3_Junction => "C3 Funnel — hold Junction plaza",
            MissionNodeId.S4_WarehouseApproach => "C4 Door — break warehouse approach",
            MissionNodeId.S5_Hideout => "N1 — clear prelude, then The Watcher",
            _ => "Advance the chain"
        };

        void OnGUI()
        {
            GUI.Box(new Rect(12, 12, 420, 40),
                $"Riverward · {StoryLabel(Current)}" + (_waveClearedForCurrent ? " · CLEAR" : " · FIGHT"));
        }
    }
}
