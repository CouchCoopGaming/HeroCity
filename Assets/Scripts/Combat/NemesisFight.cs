using UnityEngine;
using HeroCity.Narrative;
using HeroCity.Mission;

namespace HeroCity.Combat
{
    /// <summary>N1 Blackout clash — HP 480, disengage at ≤35% OR 90s hard end. Never requires kill.</summary>
    public class NemesisFight : MonoBehaviour
    {
        const float BossHp = 480f;
        const float BossArmor = 50f;
        const float DisengageHp = 0.35f;
        const float HardEndSeconds = 90f;

        Hostile _boss;
        bool _started;
        bool _finished;
        int _phase = 1;
        float _t;
        string _line = "Blackout waits in the hideout";

        public bool Active => _started && !_finished;
        public float Elapsed => _t;

        public void Begin()
        {
            if (_started) return;
            _started = true;
            _finished = false;
            _phase = 1;
            _t = 0f;

            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Blackout";
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.position = new Vector3(293f, 1f, 248f);
            go.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
            _boss = go.AddComponent<Hostile>();
            var player = FindFirstObjectByType<HeroCity.Player.ThirdPersonMotor>();
            _boss.Configure(BossHp, true, player != null ? player.transform : null, BossArmor);
            var r = go.GetComponent<Renderer>();
            if (r != null) r.material.color = new Color(0.08f, 0.08f, 0.12f);
            _line = "VO.N1.Clash — drop Blackout to 35% or survive 90s";
            Debug.Log("[Nemesis] VO.N1.Clash — Blackout fight start");
            FindFirstObjectByType<ObjectiveHud>()?.SetObjective("N1 Clash — pressure Blackout (35% / 90s)");
        }

        void Update()
        {
            if (!_started || _finished) return;
            _t += Time.deltaTime;
            if (_boss == null)
            {
                Disengage("boss missing");
                return;
            }

            float hp = _boss.Hp01;
            if (_phase == 1 && hp < 0.6f)
            {
                _phase = 2;
                _line = "VO.N1.Grade — Blackout baits overload; keep Jolting";
                Debug.Log("[Nemesis] VO.N1.Grade");
            }

            bool hpGate = hp <= DisengageHp || !_boss.Alive;
            bool timeGate = _t >= HardEndSeconds;
            if (hpGate || timeGate)
            {
                string why = timeGate && !hpGate ? "90s hard end" : "HP ≤35%";
                Disengage(why);
            }
        }

        void Disengage(string reason)
        {
            if (_finished) return;
            _finished = true;
            _line = "VO.N1.Exit — Blackout withdraws (" + reason + ")";
            Debug.Log("[Nemesis] VO.N1.Exit — disengage: " + reason + $" t={_t:0.0}s");

            if (_boss != null)
            {
                // Never require full kill — silent despawn
                _boss.ForceDespawn();
                _boss = null;
            }

            FindFirstObjectByType<NemesisIntroHook>()?.BeginOutro();
            MissionChainController.Instance?.AdvanceToAftertaste();
            FindFirstObjectByType<ObjectiveHud>()?.SetObjective("VO.N1.Exit — C5 Aftertaste");
        }

        void OnGUI()
        {
            if (!_started) return;
            string hp = _boss != null && _boss.Alive ? $" · HP {_boss.Hp01 * 100f:0}%" : " · withdrawn";
            GUI.Box(new Rect(12, 180, 560, 28),
                _line + hp + $" · t {_t:0}s / {HardEndSeconds:0}s");
        }
    }
}
