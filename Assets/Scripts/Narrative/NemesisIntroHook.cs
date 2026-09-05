using UnityEngine;
using System.Collections;
using HeroCity.Mission;

namespace HeroCity.Narrative
{
    /// <summary>N1 Blackout outro beats after disengage. SP only. Marks story complete once.</summary>
    public class NemesisIntroHook : MonoBehaviour
    {
        bool _running;
        bool _done;
        string _beat = "waiting N1 (S5 hideout · Blackout)";

        /// <summary>Legacy name — routes to outro after fight disengage.</summary>
        public void BeginIntro() => BeginOutro();

        public void BeginOutro()
        {
            if (_running || _done) return;
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            _running = true;
            string[] steps =
            {
                "VO.N1.Exit — warehouse air thins; lights die a beat",
                "VO.N1.Hook — \"You matter to a block. One person noticed.\"",
                "C5 Aftertaste — thanks/warn; Strip blinked (stub)"
            };
            foreach (var s in steps)
            {
                _beat = s;
                Debug.Log("[Blackout] " + s);
                yield return new WaitForSeconds(1.6f);
            }
            _beat = "N1 complete — Blackout left · C5 Aftertaste";
            _running = false;
            _done = true;
            MissionChainController.Instance?.MarkStoryComplete();
            // Single SetComplete from outro — avoid double-call from fight
            FindFirstObjectByType<HeroCity.Combat.ObjectiveHud>()?.SetComplete();
        }

        void OnGUI()
        {
            GUI.Box(new Rect(12, 132, 560, 36), "Story: " + _beat);
        }
    }
}
