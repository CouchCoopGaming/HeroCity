using UnityEngine;
using HeroCity.Narrative;

namespace HeroCity.Mission
{
    /// <summary>S0→S5 mission-chain. Entering next volume advances. S5 fires Nemesis intro hook.</summary>
    public class MissionChainController : MonoBehaviour
    {
        public static MissionChainController Instance { get; private set; }
        public MissionNodeId Current { get; private set; } = MissionNodeId.S0_Boardwalk;
        public bool Complete { get; private set; }

        NemesisIntroHook _nemesis;

        void Awake()
        {
            Instance = this;
            _nemesis = FindFirstObjectByType<NemesisIntroHook>();
        }

        public void TryAdvance(MissionNodeId reached)
        {
            if (Complete) return;
            // Allow skipping ahead if player runs past a pad (graybox).
            if ((int)reached < (int)Current) return;
            if (reached != Current)
            {
                Current = reached;
                Debug.Log($"[Mission] Advanced → {Current}");
            }
            if (Current == MissionNodeId.S5_Hideout && !Complete)
            {
                Complete = true;
                _nemesis?.BeginIntro();
            }
        }

        static string StoryLabel(MissionNodeId id) => id switch
        {
            MissionNodeId.S0_Boardwalk => "C1 Call (S0)",
            MissionNodeId.S1_Bodega => "C1b/C2 Pattern (S1)",
            MissionNodeId.S2_AlleyRoof => "C2/C3 Trail (S2)",
            MissionNodeId.S3_Junction => "C3 Funnel (S3)",
            MissionNodeId.S4_WarehouseApproach => "C4 Door (S4)",
            MissionNodeId.S5_Hideout => "N1 Hideout (S5)",
            _ => id.ToString()
        };

        void OnGUI()
        {
            GUI.Box(new Rect(12, 12, 420, 48),
                $"Riverward · {StoryLabel(Current)}" + (Complete ? " · N1/C5" : ""));
        }
    }
}
