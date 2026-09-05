using UnityEngine;
using HeroCity.Mission;
using HeroCity.Player;

namespace HeroCity.Combat
{
    public class ObjectiveHud : MonoBehaviour
    {
        bool _complete;
        PlayerHealth _hp;
        string _obj = "Enter S0 Boardwalk — C1 Call";

        void Start()
        {
            var p = FindFirstObjectByType<ThirdPersonMotor>();
            if (p != null) _hp = p.GetComponent<PlayerHealth>();
        }

        public void SetObjective(string s) => _obj = s;
        public void SetComplete()
        {
            _complete = true;
            _obj = "SLICE COMPLETE — Esc menu · R retry";
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                bool unlock = Cursor.lockState == CursorLockMode.Locked;
                Cursor.lockState = unlock ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = unlock;
                Time.timeScale = unlock ? 0f : 1f;
            }
        }

        void OnGUI()
        {
            float cx = Screen.width * 0.5f;
            GUI.Box(new Rect(cx - 260, 8, 520, 50),
                (_complete ? "[DONE] " : "[OBJ] ") + _obj);
            if (_hp != null)
                GUI.Box(new Rect(12, Screen.height - 36, 200, 24), $"HP {_hp.Hp01*100:0}%");
            if (_hp != null && !_hp.Alive)
                GUI.Box(new Rect(cx - 100, Screen.height * 0.5f, 200, 40), "DOWN — press R");
            GUI.Label(new Rect(Screen.width - 220, Screen.height - 28, 210, 24), "Esc pause · K skip wave");
        }
    }
}
