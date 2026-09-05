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
            _obj = "SLICE COMPLETE — Esc menu · R soft CK · Shift+R reload";
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
            GUI.Box(new Rect(cx - 280, 8, 560, 50),
                (_complete ? "[DONE] " : "[OBJ] ") + _obj);

            var chain = MissionChainController.Instance;
            string door = chain == null ? "?" : (chain.DoorUnlocked ? "DOOR UNLOCKED" : "DOOR LOCKED");
            string ck = SoftCheckpoint.HasCheckpoint
                ? $"CK {SoftCheckpoint.Node}"
                : "CK —";
            GUI.Box(new Rect(12, Screen.height - 64, 320, 24), $"{door} · {ck}");

            if (_hp != null)
                GUI.Box(new Rect(12, Screen.height - 36, 200, 24), $"HP {_hp.Hp01 * 100f:0}%");
            if (_hp != null && !_hp.Alive)
                GUI.Box(new Rect(cx - 140, Screen.height * 0.5f, 280, 40), "DOWN — R soft CK · Shift+R reload");

            var enc = EncounterDirector.Instance;
            if (enc != null)
                GUI.Label(new Rect(Screen.width - 360, Screen.height - 28, 350, 24),
                    enc.Status + " · Esc · K skip");
            else
                GUI.Label(new Rect(Screen.width - 220, Screen.height - 28, 210, 24), "Esc pause · K skip wave");
        }
    }
}
