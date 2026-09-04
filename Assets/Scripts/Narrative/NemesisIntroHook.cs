using UnityEngine;
using System.Collections;

namespace HeroCity.Narrative
{
    /// <summary>Placeholder Blackout intro: Arrival→Reveal→Grade→Clash→Exit→Hook. Leave alive.</summary>
    public class NemesisIntroHook : MonoBehaviour
    {
        bool _running;
        string _beat = "waiting S5";

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
                "N1 Reveal — Blackout (Surge foil) steps from breaker shadow",
                "N1 Grade — cites S0–S3 / ozone tells",
                "N1 Clash — teach exchange (placeholder; leave alive)",
                "N1 Exit — breaker calling card; lights die a beat",
                "N1 Hook — \"You matter to a block. One person noticed.\""
            };
            foreach (var s in steps)
            {
                _beat = s;
                Debug.Log("[Nemesis] " + s);
                yield return new WaitForSeconds(1.6f);
            }
            _beat = "Intro complete — nemesis left alive";
            _running = false;
        }

        void OnGUI()
        {
            GUI.Box(new Rect(12, 132, 520, 36), "Nemesis: " + _beat);
        }
    }
}
