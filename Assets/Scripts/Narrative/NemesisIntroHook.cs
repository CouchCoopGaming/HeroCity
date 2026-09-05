using UnityEngine;
using System.Collections;

namespace HeroCity.Narrative
{
    /// <summary>N1 Blackout beats — VO.N1.* stubs. Slideshow ≤12s total. SP only.</summary>
    public class NemesisIntroHook : MonoBehaviour
    {
        bool _running;
        string _beat = "waiting N1 (S5 hideout · Blackout)";

        /// <summary>Optional pre-clash tease — keep short.</summary>
        public void BeginIntro()
        {
            if (_running) return;
            StartCoroutine(RunReveal());
        }

        /// <summary>Post-disengage outro — VO.N1.Exit → C5. ≤12s total VO.</summary>
        public void BeginOutro()
        {
            if (_running) return;
            StartCoroutine(RunOutro());
        }

        IEnumerator RunReveal()
        {
            _running = true;
            // Brief reveal only — clash is playable, not slideshow
            string[] steps =
            {
                "VO.N1.Arrival — warehouse air goes thin",
                "VO.N1.Reveal — Blackout steps from breaker shadow",
            };
            foreach (var s in steps)
            {
                _beat = s;
                Debug.Log("[Nemesis] " + s);
                yield return new WaitForSeconds(1.5f);
            }
            _running = false;
        }

        IEnumerator RunOutro()
        {
            _running = true;
            // Cap ≤12s: 4 beats × ~2.5s ≈ 10s
            string[] steps =
            {
                "VO.N1.Exit — calling card; lights die a beat",
                "VO.N1.Hook — \"You matter to a block. One person noticed.\"",
                "VO.C5.Aftertaste — thanks/warn; Strip blinked (stub)",
                "N1 complete — Blackout left alive · C5 Aftertaste",
            };
            foreach (var s in steps)
            {
                _beat = s;
                Debug.Log("[Nemesis] " + s);
                yield return new WaitForSeconds(2.5f);
            }
            _running = false;
            HeroCity.Mission.MissionChainController.Instance?.MarkStoryComplete();
            FindFirstObjectByType<HeroCity.Combat.ObjectiveHud>()?.SetComplete();
        }

        void OnGUI()
        {
            GUI.Box(new Rect(12, 132, 560, 36), "Story: " + _beat);
        }
    }
}
