using UnityEngine;
using UnityEngine.SceneManagement;
using HeroCity.Mission;

namespace HeroCity.Combat
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] float maxHp = 100f;
        float _hp;
        float _iFrames;
        bool _downed;
        CharacterController _cc;

        public float Hp01 => Mathf.Clamp01(_hp / maxHp);
        public bool Alive => _hp > 0f;
        public bool Downed => _downed;

        void Awake()
        {
            _hp = maxHp;
            _cc = GetComponent<CharacterController>();
        }

        void Update()
        {
            _iFrames = Mathf.Max(0f, _iFrames - Time.deltaTime);
            if (!_downed) return;

            // Shift+R — emergency full Play reload
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene("Play");
                return;
            }

            // R — soft checkpoint respawn
            if (Input.GetKeyDown(KeyCode.R))
                SoftRespawn();
        }

        public void TakeDamage(float dmg)
        {
            if (!Alive || _iFrames > 0f) return;
            _hp -= dmg;
            _iFrames = 0.45f;
            if (_hp <= 0f)
            {
                _hp = 0f;
                _downed = true;
                if (_cc != null) _cc.enabled = false;
                Debug.Log("[Player] Down — R soft CK · Shift+R full reload");
            }
        }

        public void Heal(float a) => _hp = Mathf.Min(maxHp, _hp + a);

        void SoftRespawn()
        {
            Vector3 pos = SoftCheckpoint.HasCheckpoint
                ? SoftCheckpoint.Position
                : SoftCheckpoint.DefaultPos(MissionNodeId.S0_Boardwalk);

            _hp = maxHp;
            _downed = false;
            _iFrames = 1.2f;

            if (_cc != null)
            {
                _cc.enabled = false;
                transform.position = pos;
                _cc.enabled = true;
            }
            else
            {
                transform.position = pos;
            }

            // Restart wave for Current if still fighting, else keep mission Current
            var enc = EncounterDirector.Instance;
            var chain = MissionChainController.Instance;
            if (enc != null && chain != null)
            {
                if (!enc.WaveClear || enc.WaveActive)
                    enc.RestartWaveForCurrent();
            }

            FindFirstObjectByType<ObjectiveHud>()?.SetObjective(
                $"Respawned @ {SoftCheckpoint.Node} — continue");
            Debug.Log($"[Player] Soft respawn @ {SoftCheckpoint.Node} {pos}");
        }
    }
}
