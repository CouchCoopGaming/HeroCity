using UnityEngine;
using HeroCity.Narrative;
using HeroCity.Mission;

namespace HeroCity.Combat
{
    /// <summary>Blackout (N1) teach fight — VO beats, clash, disengage at 30% → C5 Aftertaste.</summary>
    public class NemesisFight : MonoBehaviour
    {
        Hostile _boss;
        bool _started;
        bool _finished;
        int _phase = 1;
        string _line = "Blackout waits in the hideout";

        public void Begin()
        {
            if (_started) return;
            _started = true;
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Blackout";
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.position = new Vector3(293f, 1f, 248f);
            go.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
            _boss = go.AddComponent<Hostile>();
            var player = FindFirstObjectByType<HeroCity.Player.ThirdPersonMotor>();
            _boss.Configure(220f, true, player != null ? player.transform : null);
            var r = go.GetComponent<Renderer>();
            if (r != null) r.material.color = new Color(0.1f, 0.1f, 0.15f);
            _line = "VO.N1.Reveal — Blackout steps from breaker shadow";
            Debug.Log("[Blackout] VO.N1.Reveal");
            Debug.Log("[Blackout] VO.N1.Clash — fight start");
            _line = "VO.N1.Clash — drop Blackout to 30% (teach)";
            FindFirstObjectByType<ObjectiveHud>()?.SetObjective("N1 Clash — press Blackout (VO.N1.Clash)");
        }

        void Update()
        {
            if (!_started || _finished || _boss == null) return;
            float hp = _boss.Hp01;
            if (_phase == 1 && hp < 0.6f)
            {
                _phase = 2;
                _line = "VO.N1.Grade — Blackout baits overload; keep Jolting";
                Debug.Log("[Blackout] VO.N1.Grade");
            }
            if (hp <= 0.3f || !_boss.Alive)
            {
                _finished = true;
                // Disengage: destroy boss without full kill / wave credit message
                if (_boss != null)
                {
                    if (_boss.Alive)
                        _boss.ForceDespawn();
                }
                _line = "VO.N1.Exit — Blackout disengages; calling card left";
                Debug.Log("[Blackout] VO.N1.Exit — disengage → C5_Aftertaste");
                MissionChainController.Instance?.AdvanceToAftertaste();
                FindFirstObjectByType<ObjectiveHud>()?.SetObjective("C5 Aftertaste — leave the hideout");
                FindFirstObjectByType<NemesisIntroHook>()?.BeginOutro();
            }
        }

        void OnGUI()
        {
            if (!_started) return;
            GUI.Box(new Rect(12, 180, 560, 28),
                _line + (_boss != null && _boss.Alive ? $" · HP {_boss.Hp01 * 100f:0}%" : " · gone"));
        }
    }
}
