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
            const float boxW = 640f;

            // Top center: big [OBJ] line
            string prefix = _complete ? "[DONE] " : "[OBJ] ";
            GUI.Box(new Rect(cx - boxW * 0.5f, 8, boxW, 54), prefix + _obj);

            // Subline: EncounterDirector status
            var enc = EncounterDirector.Instance;
            if (enc != null && !string.IsNullOrEmpty(enc.Status))
                GUI.Box(new Rect(cx - boxW * 0.5f, 66, boxW, 28), enc.Status);

            // Left: mission beat label from chain
            var chain = MissionChainController.Instance;
            if (chain != null)
            {
                string beat = MissionChainController.StoryLabel(chain.Current);
                string door = chain.DoorUnlocked ? " · door open" : "";
                GUI.Box(new Rect(12, 44, 300, 28), "Beat: " + beat + door);
            }

            // HP bar
            if (_hp != null)
            {
                float hpW = 220f;
                GUI.Box(new Rect(12, Screen.height - 40, hpW, 28), $"HP {_hp.Hp01 * 100f:0}%");
                GUI.DrawTexture(new Rect(16, Screen.height - 18, (hpW - 8) * _hp.Hp01, 6), Texture2D.whiteTexture);
            }

            if (_hp != null && !_hp.Alive)
                GUI.Box(new Rect(cx - 100, Screen.height * 0.5f, 200, 40), "DOWN — press R");

            // Controls hint
            GUI.Label(new Rect(Screen.width - 280, Screen.height - 28, 270, 24),
                "LMB Arc · Q/E/F SURGE · Esc pause · K skip");
        }
    }
}
