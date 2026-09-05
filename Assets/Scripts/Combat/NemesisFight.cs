using UnityEngine;
using HeroCity.Narrative;

namespace HeroCity.Combat
{
    /// <summary>The Watcher teach fight — multi-phase, leave "alive" after HP gate then outro.</summary>
    public class NemesisFight : MonoBehaviour
    {
        Hostile _boss;
        bool _started;
        bool _finished;
        int _phase = 1;
        string _line = "The Watcher waits in the hideout";

        public void Begin()
        {
            if (_started) return;
            _started = true;
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "TheWatcher";
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.position = new Vector3(293f, 1f, 248f);
            go.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
            _boss = go.AddComponent<Hostile>();
            var player = FindFirstObjectByType<HeroCity.Player.ThirdPersonMotor>();
            _boss.Configure(220f, true, player != null ? player.transform : null);
            var r = go.GetComponent<Renderer>();
            if (r != null) r.material.color = new Color(0.1f, 0.1f, 0.15f);
            _line = "N1 Clash — drop The Watcher to 30% (teach)";
            Debug.Log("[Nemesis] Fight start");
        }

        void Update()
        {
            if (!_started || _finished || _boss == null) return;
            float hp = _boss.Hp01;
            if (_phase == 1 && hp < 0.6f)
            {
                _phase = 2;
                _line = "N1 Grade — The Watcher baits overload; keep Jolting";
            }
            if (hp <= 0.3f || !_boss.Alive)
            {
                _finished = true;
                if (_boss.Alive) _boss.TakeDamage(9999f, true);
                _line = "N1 Exit — calling card left; The Watcher withdraws";
                FindFirstObjectByType<NemesisIntroHook>()?.BeginIntro();
                FindFirstObjectByType<ObjectiveHud>()?.SetComplete();
            }
        }

        void OnGUI()
        {
            if (!_started) return;
            GUI.Box(new Rect(12, 180, 520, 28), _line + (_boss != null && _boss.Alive ? $" · HP {_boss.Hp01*100:0}%" : ""));
        }
    }
}
