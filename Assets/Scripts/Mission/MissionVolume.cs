using UnityEngine;

namespace HeroCity.Mission
{
    [RequireComponent(typeof(Collider))]
    public class MissionVolume : MonoBehaviour
    {
        [SerializeField] MissionNodeId node;

        public void Configure(MissionNodeId id)
        {
            node = id;
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") && other.GetComponentInParent<HeroCity.Player.ThirdPersonMotor>() == null)
                return;
            MissionChainController.Instance?.TryAdvance(node);
        }
    }
}
