using UnityEngine;
using HeroCity.Surge;

namespace HeroCity.Combat
{
    /// <summary>Simple SP trash/elite. Takes Arc primary + SURGE. No net.</summary>
    public class Hostile : MonoBehaviour
    {
        [SerializeField] float maxHp = 40f;
        [SerializeField] float move = 3.2f;
        [SerializeField] float damage = 6f;
        [SerializeField] float hitInterval = 1.1f;
        [SerializeField] bool elite;

        float _hp;
        float _jolt;
        float _hitCd;
        Transform _player;
        CharacterController _cc;
        float _vy;
        public bool Alive => _hp > 0f;
        public bool IsElite => elite;
        public float Hp01 => Mathf.Clamp01(_hp / maxHp);

        public void Configure(float hp, bool isElite, Transform player)
        {
            maxHp = hp;
            _hp = hp;
            elite = isElite;
            _player = player;
            var r = GetComponent<Renderer>();
            if (r != null)
                r.material.color = isElite ? new Color(0.7f, 0.2f, 0.85f) : new Color(0.75f, 0.35f, 0.25f);
        }

        void Awake()
        {
            _hp = maxHp;
            _cc = GetComponent<CharacterController>();
            if (_cc == null) _cc = gameObject.AddComponent<CharacterController>();
            _cc.height = 1.8f; _cc.radius = 0.4f; _cc.center = new Vector3(0, 0.9f, 0);
        }

        void Start()
        {
            if (_player == null)
            {
                var m = FindFirstObjectByType<HeroCity.Player.ThirdPersonMotor>();
                if (m != null) _player = m.transform;
            }
        }

        void Update()
        {
            if (!Alive || _player == null) return;
            _jolt = Mathf.Max(0f, _jolt - Time.deltaTime * 0.35f);
            Vector3 to = _player.position - transform.position;
            to.y = 0f;
            float dist = to.magnitude;
            if (dist > 0.2f)
            {
                Vector3 dir = to.normalized;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
                float spd = move * (_jolt > 0.2f ? 0.7f : 1f);
                if (_cc != null && _cc.enabled)
                {
                    if (_cc.isGrounded) _vy = -2f; else _vy -= 22f * Time.deltaTime;
                    Vector3 mv = dir * spd; mv.y = _vy;
                    _cc.Move(mv * Time.deltaTime);
                }
            }

            _hitCd -= Time.deltaTime;
            if (dist < 1.6f && _hitCd <= 0f)
            {
                _hitCd = hitInterval;
                var ph = _player.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(damage * (elite ? 1.35f : 1f));
            }
        }

        public void ApplyJolt(float amount)
        {
            _jolt = Mathf.Clamp(_jolt + amount, 0f, 5f);
        }

        public void TakeDamage(float amount, bool fromSurge = false)
        {
            if (!Alive) return;
            float mul = 1f + _jolt * 0.12f;
            if (fromSurge) mul += 0.25f;
            _hp -= amount * mul;
            if (_hp <= 0f)
            {
                _hp = 0f;
                EncounterDirector.Instance?.OnHostileDied(this);
                // Orb stub for Capacitor / Surge
                var surge = FindFirstObjectByType<SurgeController>();
                surge?.NotifyPoweredKill(elite);
                Destroy(gameObject, 0.05f);
            }
        }

        void OnGUI()
        {
            if (!Alive || Camera.main == null) return;
            Vector3 sp = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.2f);
            if (sp.z < 0f) return;
            float w = 48f;
            GUI.Box(new Rect(sp.x - w * 0.5f, Screen.height - sp.y, w, 8), "");
            GUI.DrawTexture(new Rect(sp.x - w * 0.5f, Screen.height - sp.y, w * Hp01, 8), Texture2D.whiteTexture);
        }
    }
}
