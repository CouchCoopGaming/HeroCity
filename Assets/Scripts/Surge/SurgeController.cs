using UnityEngine;
using HeroCity.Core;

namespace HeroCity.Surge
{
    /// <summary>
    /// SURGE Ability Kit v0 stubs (SP only). Stub order: Cap Mine+Cell → Arc Seed → Field Puck → supers last.
    /// </summary>
    public class SurgeController : MonoBehaviour
    {
        [SerializeField] SurgeVariantId variant = SurgeVariantId.Chainjack;

        // Kit v0 CDs (seconds)
        const float ChainjackGrenadeCd = 20f;
        const float ChainjackUtilityCd = 12f;
        const float ChainjackSuperCd = 270f; // 4:30

        const float CapacitorGrenadeCd = 25f;
        const float CapacitorUtilityCd = 18f;
        const float CapacitorSuperCd = 300f; // 5:00

        const float StaticGrenadeCd = 22f;
        const float StaticUtilityCd = 15f;
        const float StaticSuperCd = 300f; // 5:00

        float _g, _u, _s;
        int _cells; // Capacitor 0–5
        int _zipCharges = 2;
        string _last = "-";
        string _implOrder = "stub focus: Cap Mine+Cell → Arc Seed → Field Puck → supers";

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

            if (Input.GetKeyDown(KeyCode.Q) && _g <= 0f) { FireGrenade(); }
            if (Input.GetKeyDown(KeyCode.E) && _u <= 0f) { FireUtility(); }
            if (Input.GetKeyDown(KeyCode.F) && _s <= 0f) { FireSuper(); }
            // Capacitor Cell dump bind (kit: dump = nova path) — R while Cells>=1 for stub dump
            if (variant == SurgeVariantId.Capacitor && Input.GetKeyDown(KeyCode.R) && _cells > 0)
                DumpCells();
        }

        float GrenadeCd() => variant switch
        {
            SurgeVariantId.Chainjack => ChainjackGrenadeCd,
            SurgeVariantId.Capacitor => CapacitorGrenadeCd,
            _ => StaticGrenadeCd
        };

        float UtilityCd() => variant switch
        {
            SurgeVariantId.Chainjack => ChainjackUtilityCd,
            SurgeVariantId.Capacitor => CapacitorUtilityCd,
            _ => StaticUtilityCd
        };

        float SuperCd() => variant switch
        {
            SurgeVariantId.Chainjack => ChainjackSuperCd,
            SurgeVariantId.Capacitor => CapacitorSuperCd,
            _ => StaticSuperCd
        };

        void FireGrenade()
        {
            _g = GrenadeCd();
            switch (variant)
            {
                case SurgeVariantId.Capacitor:
                    // Stub priority #1 — Cap Mine
                    _last = $"Cap Mine (sticky, arm 0.6s, scales Cells {_cells}/3) · CD {CapacitorGrenadeCd}s";
                    break;
                case SurgeVariantId.Chainjack:
                    // Stub priority #2 — Arc Seed
                    _last = $"Arc Seed (Jolt + chain 3 @ 8m) · CD {ChainjackGrenadeCd}s";
                    break;
                default:
                    // Stub priority #3 — Field Puck
                    _last = $"Field Puck (6s / 5m Field, 30% slow, +15% vs Jolted) · CD {StaticGrenadeCd}s";
                    break;
            }
            Debug.Log("[SURGE] " + _last);
        }

        void FireUtility()
        {
            switch (variant)
            {
                case SurgeVariantId.Capacitor:
                    _u = CapacitorUtilityCd;
                    _cells = Mathf.Min(5, _cells + 1);
                    _last = $"Magazine Overcharge (+1 Cell → {_cells}/5) · CD {CapacitorUtilityCd}s";
                    break;
                case SurgeVariantId.Chainjack:
                    if (_zipCharges <= 0) { _last = "Zip Contact — no charges"; return; }
                    _zipCharges--;
                    _u = ChainjackUtilityCd;
                    _last = $"Zip Contact (1.5s Jolt shots) · charges {_zipCharges}/2 · CD {ChainjackUtilityCd}s";
                    if (_zipCharges <= 0) _zipCharges = 2; // stub recharge on empty
                    break;
                default:
                    _u = StaticUtilityCd;
                    _last = $"Static Pulse (2.5s overshield + push, Jolt @ 4m) · CD {StaticUtilityCd}s";
                    break;
            }
            Debug.Log("[SURGE] " + _last);
        }

        void FireSuper()
        {
            // Stub priority #4 — supers last
            switch (variant)
            {
                case SurgeVariantId.Capacitor:
                    if (_cells < 3) { _last = $"Full Cell Nova blocked — need ≥3 Cells (have {_cells})"; return; }
                    _s = CapacitorSuperCd;
                    _last = $"Full Cell Nova ({_cells} Cells) · CD 5:00";
                    _cells = 0;
                    break;
                case SurgeVariantId.Chainjack:
                    _s = ChainjackSuperCd;
                    _last = "Storm Walk (8s auto-chain Jolted @ 12m / 0.4s) · CD 4:30";
                    break;
                default:
                    _s = StaticSuperCd;
                    _last = "Arena Cage (10s dome) · CD 5:00";
                    break;
            }
            Debug.Log("[SURGE] " + _last);
        }

        void DumpCells()
        {
            _last = $"Cell dump stub ×{_cells} (bind toward Cap Mine / nova path)";
            Debug.Log("[SURGE] " + _last);
            _cells = 0;
        }

        void OnGUI()
        {
            GUI.Box(new Rect(12, 68, 560, 72),
                $"SURGE {variant} · Cells {_cells} · Q {_g:0} · E {_u:0} · F {_s:0}\n{_last}\n{_implOrder}");
        }
    }
}
