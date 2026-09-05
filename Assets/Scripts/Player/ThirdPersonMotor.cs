using UnityEngine;

namespace HeroCity.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonMotor : MonoBehaviour
    {
        [SerializeField] float walk = 5.5f;
        [SerializeField] float sprint = 8.5f;
        [SerializeField] float jump = 7.5f;
        [SerializeField] float gravity = 22f;
        [SerializeField] float turn = 720f;

        CharacterController _cc;
        ThirdPersonCamera _cam;
        float _vy;

        public Vector3 PlanarVelocity { get; private set; }

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _cam = FindFirstObjectByType<ThirdPersonCamera>();
        }

        void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 input = new Vector3(h, 0f, v);
            if (input.sqrMagnitude > 1f) input.Normalize();

            Vector3 forward = _cam != null ? _cam.PlanarForward : Vector3.forward;
            Vector3 right = _cam != null ? _cam.PlanarRight : Vector3.right;
            Vector3 wish = forward * input.z + right * input.x;
            if (wish.sqrMagnitude > 0.001f)
            {
                Quaternion look = Quaternion.LookRotation(wish, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turn * Time.deltaTime);
            }

            float speed = Input.GetKey(KeyCode.LeftShift) ? sprint : walk;
            PlanarVelocity = wish * speed;

            if (_cc.isGrounded)
            {
                _vy = -2f;
                if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
                    _vy = jump;
            }
            else _vy -= gravity * Time.deltaTime;

            Vector3 move = PlanarVelocity;
            move.y = _vy;
            _cc.Move(move * Time.deltaTime);
        }
    }
}
