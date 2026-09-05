using UnityEngine;

namespace HeroCity.Player
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0f, 2.2f, -5.5f);
        [SerializeField] float sens = 2.2f;
        [SerializeField] float pitchMin = -25f;
        [SerializeField] float pitchMax = 55f;

        float _yaw, _pitch = 12f;

        public Vector3 PlanarForward
        {
            get
            {
                Vector3 f = transform.forward; f.y = 0f;
                return f.sqrMagnitude > 0.001f ? f.normalized : Vector3.forward;
            }
        }
        public Vector3 PlanarRight => Vector3.Cross(Vector3.up, PlanarForward).normalized;

        public void SetTarget(Transform t) => target = t;

        void LateUpdate()
        {
            if (target == null) return;
            _yaw += Input.GetAxis("Mouse X") * sens;
            _pitch -= Input.GetAxis("Mouse Y") * sens;
            _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);
            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 pos = target.position + rot * offset;
            transform.SetPositionAndRotation(pos, rot);
            transform.LookAt(target.position + Vector3.up * 1.4f);
        }
    }
}
