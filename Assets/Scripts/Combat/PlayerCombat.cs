using UnityEngine;
using HeroCity.Surge;

namespace HeroCity.Combat
{
    /// <summary>LMB Arc primary — applies Jolt. RMB Cap Mine / Arc Seed / Field Puck shortcut via Surge Q still primary.</summary>
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] float range = 28f;
        [SerializeField] float damage = 9f;
        [SerializeField] float fireInterval = 0.18f;
        [SerializeField] float joltPerHit = 0.55f;
        float _cd;
        Camera _cam;

        void Start() => _cam = Camera.main;

        void Update()
        {
            _cd -= Time.deltaTime;
            if (Input.GetMouseButton(0) && _cd <= 0f)
            {
                _cd = fireInterval;
                Fire();
            }
        }

        void Fire()
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;
            Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out var hit, range))
            {
                var h = hit.collider.GetComponentInParent<Hostile>();
                if (h != null && h.Alive)
                {
                    h.ApplyJolt(joltPerHit);
                    h.TakeDamage(damage, false);
                }
            }
        }
    }
}
