using UnityEngine;
using HeroCity.Core;
using HeroCity.Combat;

namespace HeroCity.Surge
{
    /// <summary>
    /// SURGE kit — combat-affecting stubs: Jolt, Cells+dump, Cap Mine scale, Field Puck zone, Zip CD refill.
    /// Guns-only must still clear; SURGE is feel not door key. Variant locked at boot via GameFlow.
    /// </summary>
    public class SurgeController : MonoBehaviour
    {
        [SerializeField] SurgeVariantId variant = SurgeVariantId.Chainjack;
        bool _variantLocked;

        const float ChainjackGrenadeCd = 20f;
        const float ChainjackUtilityCd = 12f;
        const float ChainjackSuperCd = 270f;

        const float CapacitorGrenadeCd = 25f;
        const float CapacitorUtilityCd = 18f;
        const float CapacitorSuperCd = 300f;

        const float StaticGrenadeCd = 22f;
        const float StaticUtilityCd = 15f;
        const float StaticSuperCd = 300f;

        const float ArcSeedDmg = 28f;
        const float CapMineBase = 32f;
        const float FieldRadius = 5f;
        const float FieldDuration = 6f;

        float _g, _u, _s;
        int _cells; // Capacitor 0–5
        int _zipCharges = 2;
        float _zipRefill; // refill timer when charges < 2
        float _cellStarveTimer;
        string _last = "-";
        FieldZone _activeField;

        public SurgeVariantId Variant => variant;
        public int Cells => _cells;

        void Start()
        {
            LockVariantFromBoot();
        }

        void LockVariantFromBoot()
        {
            if (_variantLocked) return;
            if (GameFlow.Instance != null)
                variant = GameFlow.Instance.SelectedSurge;
            else if (PlayerPrefs.HasKey("HC.Surge"))
                variant = (SurgeVariantId)PlayerPrefs.GetInt("HC.Surge", 0);
            _variantLocked = true;
            Debug.Log($"[SURGE] Variant locked at boot: {variant}");
        }

        void Update()
        {
            float dt = Time.deltaTime;
            _g = Mathf.Max(0f, _g - dt);
            _u = Mathf.Max(0f, _u - dt);
            _s = Mathf.Max(0f, _s - dt);

            // Zip charges refill on CD (not stuck at 0)
            if (variant == SurgeVariantId.Chainjack && _zipCharges < 2)
            {
                _zipRefill -= dt;
                if (_zipRefill <= 0f)
                {
                    _zipCharges = Mathf.Min(2, _zipCharges + 1);
                    _zipRefill = ChainjackUtilityCd;
                    if (_zipCharges < 2) { /* keep ticking */ }
                    Debug.Log($"[SURGE] Zip refill → {_zipCharges}/2");
                }
            }

            // Cell floor if starved (soft refill) — Capacitor never stuck at 0 forever
            if (variant == SurgeVariantId.Capacitor)
            {
                if (_cells <= 0)
                {
                    _cellStarveTimer += dt;
                    if (_cellStarveTimer >= 18f)
                    {
                        _cells = 1;
                        _cellStarveTimer = 0f;
                        _last = "Cell floor soft-refill → 1 (starved)";
                        Debug.Log("[SURGE] " + _last);
                    }
                }
                else _cellStarveTimer = 0f;
            }

            if (Input.GetKeyDown(KeyCode.Q) && _g <= 0f) FireGrenade();
            if (Input.GetKeyDown(KeyCode.E) && _u <= 0f) FireUtility();
            if (Input.GetKeyDown(KeyCode.F) && _s <= 0f) FireSuper();
            // Capacitor Cell dump — R (PlayerHealth soft-respawn only when downed)
            if (variant == SurgeVariantId.Capacitor && Input.GetKeyDown(KeyCode.R) && _cells > 0)
            {
                var ph = GetComponent<PlayerHealth>();
                if (ph == null || ph.Alive)
                    DumpCells();
            }
        }

        float GrenadeCd() => variant switch
        {
            SurgeVariantId.Chainjack => ChainjackGrenadeCd,
            SurgeVariantId.Capacitor => CapacitorGrenadeCd,
            _ => StaticGrenadeCd
        };

        void FireGrenade()
        {
            _g = GrenadeCd();
            switch (variant)
            {
                case SurgeVariantId.Capacitor:
                    CapMine();
                    break;
                case SurgeVariantId.Chainjack:
                    ArcSeed();
                    break;
                default:
                    FieldPuck();
                    break;
            }
            Debug.Log("[SURGE] " + _last);
        }

        void CapMine()
        {
            // Scales with Cells — still works at 0 (guns+base mine), Cells amplify
            float scale = 1f + _cells * 0.35f;
            float dmg = CapMineBase * scale;
            Vector3 origin = transform.position + transform.forward * 2.5f + Vector3.up;
            int hit = 0;
            foreach (var h in FindObjectsByType<Hostile>(FindObjectsSortMode.None))
            {
                if (h == null || !h.Alive) continue;
                if ((h.transform.position - origin).sqrMagnitude > 7f * 7f) continue;
                h.ApplyJolt(0.9f + _cells * 0.15f);
                h.ApplyCellCharge(_cells * 0.4f);
                h.TakeDamage(dmg, true);
                hit++;
            }
            _last = $"Cap Mine · Cells {_cells}/5 · dmg {dmg:0} · hit {hit} · CD {CapacitorGrenadeCd}s";
        }

        void ArcSeed()
        {
            Vector3 origin = transform.position + transform.forward * 2f + Vector3.up;
            // Collect nearby, chain up to 3
            var list = new System.Collections.Generic.List<Hostile>();
            foreach (var h in FindObjectsByType<Hostile>(FindObjectsSortMode.None))
            {
                if (h == null || !h.Alive) continue;
                if ((h.transform.position - origin).sqrMagnitude > 10f * 10f) continue;
                list.Add(h);
            }
            list.Sort((a, b) =>
                (a.transform.position - origin).sqrMagnitude.CompareTo(
                    (b.transform.position - origin).sqrMagnitude));
            int n = Mathf.Min(3, list.Count);
            for (int i = 0; i < n; i++)
            {
                list[i].ApplyJolt(1.5f);
                list[i].TakeDamage(ArcSeedDmg * (1f - i * 0.12f), true);
            }
            _last = $"Arc Seed (Jolt + chain {n}@8–10m) · CD {ChainjackGrenadeCd}s";
        }

        void FieldPuck()
        {
            Vector3 pos = transform.position + transform.forward * 3f;
            pos.y = transform.position.y;
            if (_activeField != null) Destroy(_activeField.gameObject);
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "FieldPuck_Zone";
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.position = pos + Vector3.up * 0.05f;
            go.transform.localScale = new Vector3(FieldRadius * 2f, 0.05f, FieldRadius * 2f);
            var r = go.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = new Color(0.2f, 0.55f, 0.9f, 0.45f);
            }
            _activeField = go.AddComponent<FieldZone>();
            _activeField.Init(FieldRadius, FieldDuration, 0.30f, 0.15f);
            _last = $"Field Puck ({FieldDuration:0}s / {FieldRadius:0}m · 30% slow · +15% vs Jolted) · CD {StaticGrenadeCd}s";
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
                    if (_zipCharges <= 0)
                    {
                        _last = "Zip Contact — no charges (refilling)";
                        return;
                    }
                    _zipCharges--;
                    _u = ChainjackUtilityCd;
                    if (_zipCharges < 2 && _zipRefill <= 0f)
                        _zipRefill = ChainjackUtilityCd;
                    // Brief Jolt on nearby hostiles (1.5s window feel)
                    Vector3 origin = transform.position;
                    foreach (var h in FindObjectsByType<Hostile>(FindObjectsSortMode.None))
                    {
                        if (h == null || !h.Alive) continue;
                        if ((h.transform.position - origin).sqrMagnitude > 6f * 6f) continue;
                        h.ApplyJolt(1.2f);
                    }
                    _last = $"Zip Contact · charges {_zipCharges}/2 · CD {ChainjackUtilityCd}s";
                    break;
                default:
                    _u = StaticUtilityCd;
                    foreach (var h in FindObjectsByType<Hostile>(FindObjectsSortMode.None))
                    {
                        if (h == null || !h.Alive) continue;
                        if ((h.transform.position - transform.position).sqrMagnitude > 4f * 4f) continue;
                        h.ApplyJolt(0.8f);
                    }
                    _last = $"Static Pulse (overshield stub + Jolt @4m) · CD {StaticUtilityCd}s";
                    break;
            }
            Debug.Log("[SURGE] " + _last);
        }

        void FireSuper()
        {
            switch (variant)
            {
                case SurgeVariantId.Capacitor:
                    if (_cells < 3) { _last = $"Full Cell Nova blocked — need ≥3 Cells (have {_cells})"; return; }
                    _s = CapacitorSuperCd;
                    float nova = 55f + _cells * 18f;
                    foreach (var h in FindObjectsByType<Hostile>(FindObjectsSortMode.None))
                    {
                        if (h == null || !h.Alive) continue;
                        if ((h.transform.position - transform.position).sqrMagnitude > 12f * 12f) continue;
                        h.ApplyJolt(2f);
                        h.TakeDamage(nova, true);
                    }
                    _last = $"Full Cell Nova ({_cells} Cells · {nova:0} dmg) · CD 5:00";
                    _cells = 0;
                    break;
                case SurgeVariantId.Chainjack:
                    _s = ChainjackSuperCd;
                    foreach (var h in FindObjectsByType<Hostile>(FindObjectsSortMode.None))
                    {
                        if (h == null || !h.Alive) continue;
                        if ((h.transform.position - transform.position).sqrMagnitude > 12f * 12f) continue;
                        h.ApplyJolt(2f);
                        h.TakeDamage(40f, true);
                    }
                    _last = "Storm Walk (auto-chain Jolted stub) · CD 4:30";
                    break;
                default:
                    _s = StaticSuperCd;
                    FieldPuck();
                    _g = 0f; // already set Field via helper; keep super CD
                    _s = StaticSuperCd;
                    _last = "Arena Cage (Field dome stub) · CD 5:00";
                    break;
            }
            Debug.Log("[SURGE] " + _last);
        }

        void DumpCells()
        {
            if (_cells <= 0) return;
            float dmg = 14f * _cells;
            int spent = _cells;
            Vector3 origin = transform.position;
            foreach (var h in FindObjectsByType<Hostile>(FindObjectsSortMode.None))
            {
                if (h == null || !h.Alive) continue;
                if ((h.transform.position - origin).sqrMagnitude > 8f * 8f) continue;
                h.ApplyJolt(0.6f * spent);
                h.TakeDamage(dmg, true);
            }
            _last = $"Cell dump ×{spent} → {dmg:0} AoE";
            Debug.Log("[SURGE] " + _last);
            _cells = 0;
        }

        public void NotifyPoweredKill(bool elite)
        {
            if (variant == SurgeVariantId.Capacitor)
                _cells = Mathf.Min(5, _cells + (elite ? 2 : 1));
        }

        void OnGUI()
        {
            string zip = variant == SurgeVariantId.Chainjack ? $" · Zip {_zipCharges}/2" : "";
            GUI.Box(new Rect(12, 68, 560, 56),
                $"SURGE {variant} · Cells {_cells}/5{zip} · Q {_g:0} · E {_u:0} · F {_s:0}\n{_last}");
        }
    }

    /// <summary>5m / 6s Field zone — 30% slow, +15% dmg vs Jolted (via Hostile field flag).</summary>
    public class FieldZone : MonoBehaviour
    {
        float _radius;
        float _life;
        float _slow;
        float _tick;

        public void Init(float radius, float duration, float slow01, float ampIgnored)
        {
            _radius = radius;
            _life = duration;
            _slow = slow01;
        }

        void Update()
        {
            _life -= Time.deltaTime;
            _tick -= Time.deltaTime;
            if (_tick <= 0f)
            {
                _tick = 0.25f;
                foreach (var h in FindObjectsByType<Hostile>(FindObjectsSortMode.None))
                {
                    if (h == null || !h.Alive) continue;
                    if ((h.transform.position - transform.position).sqrMagnitude > _radius * _radius) continue;
                    h.ApplyFieldSlow(_slow);
                    // Small tick + amp handled in Hostile.TakeDamage when Jolted+Field
                    if (h.IsJolted)
                        h.TakeDamage(3.5f, true);
                    else
                        h.TakeDamage(1.5f, true);
                }
            }
            if (_life <= 0f) Destroy(gameObject);
        }
    }
}
