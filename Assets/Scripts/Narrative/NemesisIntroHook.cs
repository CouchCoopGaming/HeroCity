using UnityEngine;
using System.Collections;

namespace HeroCity.Narrative
{
    /// <summary>N1 hideout intro — The Watcher (placeholder). SP only, no co-op story.</summary>
    public class NemesisIntroHook : MonoBehaviour
    {
        bool _running;
        string _beat = "waiting N1 (S5 hideout)";

        public void BeginIntro()
        {
            if (_running) return;
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            _running = true;
            string[] steps =
            {
                "N1 Arrival — warehouse air goes thin",
                "N1 Reveal — The Watcher steps from breaker shadow",
                "N1 Grade — cites C1–C3 / ozone tells along the chain",
                "N1 Clash — teach exchange (placeholder; leave alive)",
                "N1 Exit — calling card; lights die a beat",
                "N1 Hook — \"You matter to a block. One person noticed.\"",
                "C5 Aftertaste — thanks/warn; Strip blinked (stub)"
            };
            foreach (var s in steps)
            {
                _beat = s;
                Debug.Log("[Nemesis] " + s);
                yield return new WaitForSeconds(1.6f);
            }
            _beat = "N1 complete — The Watcher left alive · C5 stub fired";
            _running = false;
        }

        void OnGUI()
        {
            GUI.Box(new Rect(12, 132, 560, 36), "Story: " + _beat);
        }
    }
}
