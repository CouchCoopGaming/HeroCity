using UnityEngine;
using HeroCity.Combat;
using HeroCity.Narrative;

namespace HeroCity.Mission
{
    /// <summary>S0→S5→C5 chain. DoorUnlocked gates S5 after S4 wave clear. SP only.</summary>
    public class MissionChainController : MonoBehaviour
    {
        public static MissionChainController Instance { get; private set; }
        public MissionNodeId Current { get; private set; } = MissionNodeId.S0_Boardwalk;
        public bool Complete { get; private set; }
        public bool DoorUnlocked { get; private set; }
        bool _waveClearedForCurrent;

        void Awake() => Instance = this;

        public void TryAdvance(MissionNodeId reached)
        {
            if (Complete) return;

            // S5 door gate — refuse until C4 wave clears DoorUnlocked
            if (reached == MissionNodeId.S5_Hideout && !DoorUnlocked)
            {
                FindFirstObjectByType<ObjectiveHud>()?.SetObjective("Clear C4 / unlock door first");
                Debug.Log("[Mission] TryAdvance(S5) refused — !DoorUnlocked");
                return;
            }

            // C5 volume must not skip Blackout — only AdvanceToAftertaste() moves S5→C5
            if (reached == MissionNodeId.C5_Aftertaste && Current != MissionNodeId.C5_Aftertaste)
            {
                FindFirstObjectByType<ObjectiveHud>()?.SetObjective("Finish N1 Blackout first");
                Debug.Log("[Mission] TryAdvance(C5) refused — await Blackout disengage");
                return;
            }

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
                _waveClearedForCurrent = reached == MissionNodeId.C5_Aftertaste;
                Debug.Log($"[Mission] Advanced → {Current} · beat {StoryLabel(Current)}");
            }

            if (reached != MissionNodeId.C5_Aftertaste)
                EncounterDirector.Instance?.OnEnteredNode(Current);
            FindFirstObjectByType<ObjectiveHud>()?.SetObjective(StoryObjective(Current));
        }

        public void NotifyWaveCleared(MissionNodeId node)
        {
            if (node != Current) return;
            _waveClearedForCurrent = true;
            if (Current == MissionNodeId.S4_WarehouseApproach)
            {
                DoorUnlocked = true;
                Debug.Log("[Mission] DoorUnlocked = true (S4 clear) · VO.N1.Reveal pending at hideout");
                ClearDoorBlocker();
                FindFirstObjectByType<ObjectiveHud>()?.SetObjective("Door unlocked — enter hideout (S5)");
                return;
            }
            if (Current == MissionNodeId.S5_Hideout)
            {
                // NemesisFight / Blackout handles intro after prelude wave
                return;
            }
            FindFirstObjectByType<ObjectiveHud>()?.SetObjective($"Advance to {NextLabel(Current)}");
        }

        public void AdvanceToAftertaste()
        {
            Current = MissionNodeId.C5_Aftertaste;
            _waveClearedForCurrent = true;
            FindFirstObjectByType<ObjectiveHud>()?.SetObjective("C5 Aftertaste — leave the hideout");
            Debug.Log("[Mission] Advanced → C5_Aftertaste · VO.N1.Exit / aftertaste");
        }

        public void MarkStoryComplete()
        {
            Complete = true;
        }

        static void ClearDoorBlocker()
        {
            var blocker = GameObject.Find("Hideout_Door_Blocker");
            if (blocker != null)
            {
                blocker.SetActive(false);
                Debug.Log("[Mission] Hideout_Door_Blocker disabled");
            }
        }

        static string NextLabel(MissionNodeId id)
        {
            int n = Mathf.Min((int)id + 1, (int)MissionNodeId.C5_Aftertaste);
            return StoryLabel((MissionNodeId)n);
        }

        public static string StoryLabel(MissionNodeId id) => id switch
        {
            MissionNodeId.S0_Boardwalk => "C1 Call (S0)",
            MissionNodeId.S1_Bodega => "C2 Pattern (S1)",
            MissionNodeId.S2_AlleyRoof => "C3 Trail (S2)",
            MissionNodeId.S3_Junction => "C3 Funnel (S3)",
            MissionNodeId.S4_WarehouseApproach => "C4 Door (S4)",
            MissionNodeId.S5_Hideout => "N1 Blackout (S5)",
            MissionNodeId.C5_Aftertaste => "C5 Aftertaste",
            _ => id.ToString()
        };

        static string StoryObjective(MissionNodeId id) => id switch
        {
            MissionNodeId.S0_Boardwalk => "C1 Call — clear boardwalk trash, learn Arc fire (LMB) + SURGE Q/E/F",
            MissionNodeId.S1_Bodega => "C2 Pattern — clear bodega stoop pack",
            MissionNodeId.S2_AlleyRoof => "C3 Trail — alley/roof hostiles (elite in mix)",
            MissionNodeId.S3_Junction => "C3 Funnel — hold Junction plaza",
            MissionNodeId.S4_WarehouseApproach => "C4 Door — break warehouse approach · unlock hideout door",
            MissionNodeId.S5_Hideout => "N1 — clear prelude, then Blackout (VO.N1.Reveal)",
            MissionNodeId.C5_Aftertaste => "C5 Aftertaste — leave the hideout",
            _ => "Advance the chain"
        };

        void OnGUI()
        {
            // Beat label is owned by ObjectiveHud; keep a compact left badge only
            GUI.Box(new Rect(12, 12, 280, 28),
                StoryLabel(Current) + (_waveClearedForCurrent ? " · CLEAR" : " · FIGHT")
                + (DoorUnlocked ? " · DOOR" : ""));
        }
    }
}
