using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeroCity.Combat
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] float maxHp = 100f;
        float _hp;
        float _iFrames;
        public float Hp01 => Mathf.Clamp01(_hp / maxHp);
        public bool Alive => _hp > 0f;

        void Awake() => _hp = maxHp;

        void Update()
        {
            _iFrames = Mathf.Max(0f, _iFrames - Time.deltaTime);
            if (!Alive && Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene("Play");
        }

        public void TakeDamage(float dmg)
        {
            if (!Alive || _iFrames > 0f) return;
            _hp -= dmg;
            _iFrames = 0.45f;
            if (_hp <= 0f)
            {
                _hp = 0f;
                Debug.Log("[Player] Down — R to retry Play");
            }
        }

        public void Heal(float a) => _hp = Mathf.Min(maxHp, _hp + a);
    }
}
