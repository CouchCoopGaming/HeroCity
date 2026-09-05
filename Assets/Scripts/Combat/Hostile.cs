using UnityEngine;
using HeroCity.Surge;

namespace HeroCity.Combat
{
    /// <summary>SP trash/elite. CapsuleCollider for LMB raycasts. Trash strafe; Elite HoldRange + bolt.</summary>
    public class Hostile : MonoBehaviour
    {
        enum AiState { Chase, Strafe, HoldRange }

        [SerializeField] float maxHp = 80f;
        [SerializeField] float armor = 0f;
        [SerializeField] float move = 3.2f;
        [SerializeField] float damage = 6f;
        [SerializeField] float hitInterval = 1.1f;
        [SerializeField] bool elite;

        float _hp;
        float _jolt;
        float _hitCd;
        float _strafeTimer;
        float _projCd;
        float _stateTimer;
        float _fieldSlow; // 0..1 slow fraction from Field Puck
        Vector3 _strafeDir;
        AiState _state = AiState.Chase;
        Transform _player;
        CharacterController _cc;
        float _vy;

        public bool Alive => _hp > 0f;
        public bool IsElite => elite;
        public float Hp01 => maxHp > 0f ? Mathf.Clamp01(_hp / maxHp) : 0f;
        public bool IsJolted => _jolt > 0.15f;
        public float Jolt => _jolt;

        public void Configure(float hp, bool isElite, Transform player, float armorFlat = 0f)
        {
            maxHp = hp;
            _hp = hp;
            armor = armorFlat;
            elite = isElite;
            _player = player;
            _state = isElite ? AiState.HoldRange : AiState.Chase;
            _projCd = Random.Range(0.4f, 1.0f);
            _stateTimer = Random.Range(0.8f, 1.6f);
            var r = GetComponent<Renderer>();
            if (r != null)
                r.material.color = isElite ? new Color(0.7f, 0.2f, 0.85f) : new Color(0.75f, 0.35f, 0.25f);
            EnsureHitCollider();
        }

        void Awake()
        {
            _hp = maxHp;
            _cc = GetComponent<CharacterController>();
            if (_cc == null) _cc = gameObject.AddComponent<CharacterController>();
            _cc.height = 1.8f; _cc.radius = 0.4f; _cc.center = new Vector3(0, 0.9f, 0);
            // Raycast-hittable collider for PlayerCombat LMB (CC alone does not receive Physics.Raycast)
            EnsureHitCollider();
        }

        void EnsureHitCollider()
        {
            var caps = GetComponents<CapsuleCollider>();
            CapsuleCollider hit = null;
            foreach (var c in caps)
            {
                if (c != null && !c.isTrigger) { hit = c; break; }
            }
            if (hit == null)
                hit = gameObject.AddComponent<CapsuleCollider>();
            hit.isTrigger = false;
            hit.height = 1.8f;
            hit.radius = 0.4f;
            hit.center = new Vector3(0f, 0.9f, 0f);
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
            _fieldSlow = Mathf.Max(0f, _fieldSlow - Time.deltaTime); // sticky until zone refreshes
            Vector3 to = _player.position - transform.position;
            to.y = 0f;
            float dist = to.magnitude;
            Vector3 toward = dist > 0.01f ? to.normalized : transform.forward;
            float spdMul = (_jolt > 0.2f ? 0.7f : 1f) * (1f - Mathf.Clamp01(_fieldSlow));

            if (elite)
                TickElite(toward, dist, spdMul);
            else
                TickTrash(toward, dist, spdMul);

            _hitCd -= Time.deltaTime;
            if (dist < 1.6f && _hitCd <= 0f)
            {
                _hitCd = hitInterval;
                var ph = _player.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(damage * (elite ? 1.35f : 1f));
            }
        }

        void TickTrash(Vector3 toward, float dist, float spdMul)
        {
            _stateTimer -= Time.deltaTime;
            if (_state == AiState.Chase)
            {
                if (_stateTimer <= 0f && dist > 2f)
                {
                    _state = AiState.Strafe;
                    _stateTimer = Random.Range(0.6f, 1.2f);
                    _strafeDir = Vector3.Cross(Vector3.up, toward).normalized;
                    if (Random.value < 0.5f) _strafeDir = -_strafeDir;
                }
                Move(toward * move * spdMul);
                Face(toward);
            }
            else
            {
                Vector3 dir = (_strafeDir * 0.85f + toward * 0.25f).normalized;
                Move(dir * move * 0.95f * spdMul);
                Face(toward);
                if (_stateTimer <= 0f)
                {
                    _state = AiState.Chase;
                    _stateTimer = Random.Range(1.2f, 2.4f);
                }
            }
        }

        void TickElite(Vector3 toward, float dist, float spdMul)
        {
            const float preferMin = 7f;
            const float preferMax = 12f;
            Vector3 moveDir = Vector3.zero;

            if (dist < preferMin - 0.5f)
            {
                moveDir = -toward;
                _state = AiState.HoldRange;
            }
            else if (dist > preferMax + 1f)
            {
                moveDir = toward;
                _state = AiState.Chase;
            }
            else
            {
                _state = AiState.HoldRange;
                _strafeTimer -= Time.deltaTime;
                if (_strafeTimer <= 0f)
                {
                    _strafeDir = Vector3.Cross(Vector3.up, toward).normalized;
                    if (Random.value < 0.5f) _strafeDir = -_strafeDir;
                    _strafeTimer = Random.Range(0.7f, 1.4f);
                }
                moveDir = _strafeDir;
            }

            float spd = move * (dist > preferMax ? 0.75f : 0.9f) * spdMul;
            if (moveDir.sqrMagnitude > 0.01f)
                Move(moveDir.normalized * spd);
            Face(toward);

            _projCd -= Time.deltaTime;
            if (_projCd <= 0f && dist > 2.2f && dist < 22f)
            {
                _projCd = 1.4f;
                FireProjectile(toward);
            }
        }

        void FireProjectile(Vector3 toward)
        {
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "EliteBolt";
            ball.transform.position = transform.position + Vector3.up * 1.2f + toward * 0.8f;
            ball.transform.localScale = Vector3.one * 0.35f;
            Object.Destroy(ball.GetComponent<Collider>());
            var col = ball.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.5f;
            var r = ball.GetComponent<Renderer>();
            if (r != null) r.material.color = new Color(0.9f, 0.35f, 1f);
            var bolt = ball.AddComponent<EliteBolt>();
            bolt.Init(toward, 14f, damage * 0.85f, 3.5f);
        }

        void Move(Vector3 planar)
        {
            if (_cc == null || !_cc.enabled) return;
            if (_cc.isGrounded) _vy = -2f; else _vy -= 22f * Time.deltaTime;
            Vector3 mv = planar; mv.y = _vy;
            _cc.Move(mv * Time.deltaTime);
        }

        void Face(Vector3 toward)
        {
            if (toward.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toward), 8f * Time.deltaTime);
        }

        public void ApplyJolt(float amount)
        {
            _jolt = Mathf.Clamp(_jolt + amount, 0f, 5f);
        }

        public void ApplyFieldSlow(float fraction01)
        {
            _fieldSlow = Mathf.Max(_fieldSlow, Mathf.Clamp01(fraction01));
        }

        public void ApplyCellCharge(float amount)
        {
            ApplyJolt(amount * 0.5f);
        }

        public void TakeDamage(float amount, bool fromSurge = false)
        {
            if (!Alive) return;
            float mitigated = Mathf.Max(0f, amount - armor * 0.15f);
            float mul = 1f + _jolt * 0.12f;
            if (fromSurge) mul += 0.25f;
            // Field amp vs Jolted applied by FieldZone / Surge caller via fromSurge + jolt
            if (IsJolted && _fieldSlow > 0.05f) mul += 0.15f;
            _hp -= mitigated * mul;
            if (_hp <= 0f)
            {
                _hp = 0f;
                EncounterDirector.Instance?.OnHostileDied(this);
                var surge = FindFirstObjectByType<SurgeController>();
                surge?.NotifyPoweredKill(elite);
                Destroy(gameObject, 0.05f);
            }
        }

        /// <summary>Silent despawn for Blackout disengage (no kill credit).</summary>
        public void ForceDespawn()
        {
            _hp = 0f;
            Destroy(gameObject, 0.02f);
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

    public class EliteBolt : MonoBehaviour
    {
        Vector3 _vel;
        float _dmg;
        float _life;

        public void Init(Vector3 dir, float speed, float damage, float life)
        {
            _vel = dir.normalized * speed;
            _dmg = damage;
            _life = life;
        }

        void Update()
        {
            transform.position += _vel * Time.deltaTime;
            _life -= Time.deltaTime;
            if (_life <= 0f) Destroy(gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            var ph = other.GetComponentInParent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(_dmg);
                Destroy(gameObject);
            }
        }
    }
}
