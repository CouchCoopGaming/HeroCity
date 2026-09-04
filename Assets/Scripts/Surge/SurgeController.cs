using UnityEngine;
using HeroCity.Core;

namespace HeroCity.Surge
{
    /// <summary>SURGE stubs: grenade / utility / super skeletons. No net prediction.</summary>
    public class SurgeController : MonoBehaviour
    {
        [SerializeField] SurgeVariantId variant = SurgeVariantId.Chainjack;
        [SerializeField] float grenadeCd = 6f;
        [SerializeField] float utilityCd = 8f;
        [SerializeField] float superCd = 45f;

        float _g, _u, _s;
        string _last = "-";

        void Start()
        {
            if (GameFlow.Instance != null)
                variant = GameFlow.Instance.SelectedSurge;
            else if (PlayerPrefs.HasKey("HC.Surge"))
                variant = (SurgeVariantId)PlayerPrefs.GetInt("HC.Surge", 0);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            _g = Mathf.Max(0f, _g - dt);
            _u = Mathf.Max(0f, _u - dt);
            _s = Mathf.Max(0f, _s - dt);

            if (Input.GetKeyDown(KeyCode.Q) && _g <= 0f) { FireGrenade(); _g = grenadeCd; }
            if (Input.GetKeyDown(KeyCode.E) && _u <= 0f) { FireUtility(); _u = utilityCd; }
            if (Input.GetKeyDown(KeyCode.F) && _s <= 0f) { FireSuper(); _s = superCd; }
        }

        void FireGrenade()
        {
            _last = variant switch
            {
                SurgeVariantId.Chainjack => "Chainjack grenade: arc seed",
                SurgeVariantId.Capacitor => "Capacitor grenade: sticky mine",
                _ => "Static Field grenade: field puck"
            };
            Debug.Log("[SURGE] " + _last);
        }

        void FireUtility()
        {
            _last = variant switch
            {
                SurgeVariantId.Chainjack => "Chainjack utility: zip amp",
                SurgeVariantId.Capacitor => "Capacitor utility: overcharge mag",
                _ => "Static Field utility: shield pulse"
            };
            Debug.Log("[SURGE] " + _last);
        }

        void FireSuper()
        {
            _last = variant switch
            {
                SurgeVariantId.Chainjack => "Chainjack super: storm walk",
                SurgeVariantId.Capacitor => "Capacitor super: cell nova",
                _ => "Static Field super: arena cage"
            };
            Debug.Log("[SURGE] " + _last);
        }

        void OnGUI()
        {
            GUI.Box(new Rect(12, 68, 420, 56),
                $"SURGE {variant} · Q grenade {_g:0.0} · E util {_u:0.0} · F super {_s:0.0}\n{_last}");
        }
    }
}
