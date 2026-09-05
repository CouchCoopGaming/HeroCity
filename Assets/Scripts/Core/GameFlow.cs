using UnityEngine;
using UnityEngine.SceneManagement;
using HeroCity.Surge;

namespace HeroCity.Core
{
    public enum FlowState { Boot, SurgeSelect, Play }

    /// <summary>SP Boot → SURGE pick → Play. No net.</summary>
    public class GameFlow : MonoBehaviour
    {
        public static GameFlow Instance { get; private set; }
        public FlowState State { get; private set; } = FlowState.Boot;
        public SurgeVariantId SelectedSurge { get; private set; } = SurgeVariantId.Chainjack;

        [SerializeField] string bootScene = "Boot";
        [SerializeField] string playScene = "Play";
        int _cursor;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            var n = SceneManager.GetActiveScene().name;
            State = (n == playScene || n == "Play") ? FlowState.Play : FlowState.Boot;
        }

        public void GoSurgeSelect()
        {
            State = FlowState.SurgeSelect;
        }

        public void ConfirmSurgeAndPlay()
        {
            SelectedSurge = (SurgeVariantId)_cursor;
            PlayerPrefs.SetInt("HC.Surge", (int)SelectedSurge);
            PlayerPrefs.Save();
            State = FlowState.Play;
            if (SceneManager.GetActiveScene().name != playScene)
                SceneManager.LoadScene(playScene);
        }

        void Update()
        {
            if (State == FlowState.Boot)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    GoSurgeSelect();
            }
            else if (State == FlowState.SurgeSelect)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) _cursor = 0;
                if (Input.GetKeyDown(KeyCode.Alpha2)) _cursor = 1;
                if (Input.GetKeyDown(KeyCode.Alpha3)) _cursor = 2;
                if (Input.GetKeyDown(KeyCode.UpArrow)) _cursor = (_cursor + 2) % 3;
                if (Input.GetKeyDown(KeyCode.DownArrow)) _cursor = (_cursor + 1) % 3;
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    ConfirmSurgeAndPlay();
            }
        }

        void OnGUI()
        {
            float cx = Screen.width * 0.5f, cy = Screen.height * 0.5f;
            if (State == FlowState.Boot)
            {
                GUI.Box(new Rect(cx - 160, cy - 60, 320, 120), "HeroCity — SP Slice");
                GUI.Label(new Rect(cx - 140, cy - 20, 280, 40), "Riverward · Mission → Hideout · SURGE");
                if (GUI.Button(new Rect(cx - 60, cy + 20, 120, 28), "Play")) GoSurgeSelect();
            }
            else if (State == FlowState.SurgeSelect)
            {
                GUI.Box(new Rect(cx - 180, cy - 110, 360, 220), "SURGE variant");
                Draw(cx, cy - 50, 0, "1  Chainjack — pack clear");
                Draw(cx, cy - 15, 1, "2  Capacitor — boss chunk");
                Draw(cx, cy + 20, 2, "3  Static Field — lane control");
                GUI.Label(new Rect(cx - 140, cy + 60, 280, 40), "1/2/3 · Enter");
            }
        }

        void Draw(float cx, float y, int i, string label)
        {
            bool sel = _cursor == i;
            var r = new Rect(cx - 160, y, 320, 28);
            if (sel) GUI.Box(r, "");
            if (GUI.Button(r, (sel ? "> " : "  ") + label))
            {
                _cursor = i;
                ConfirmSurgeAndPlay();
            }
        }
    }
}
