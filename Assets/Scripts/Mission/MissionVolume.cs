using UnityEngine;
using HeroCity.Combat;

namespace HeroCity.Mission
{
    [RequireComponent(typeof(Collider))]
    public class MissionVolume : MonoBehaviour
    {
        [SerializeField] MissionNodeId node;
        [SerializeField] bool requireDoorUnlocked;

        public void Configure(MissionNodeId id, bool requireDoor = false)
        {
            node = id;
            requireDoorUnlocked = requireDoor;
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") && other.GetComponentInParent<HeroCity.Player.ThirdPersonMotor>() == null)
                return;

            if (requireDoorUnlocked)
            {
                var chain = MissionChainController.Instance;
                if (chain == null || !chain.DoorUnlocked)
                {
                    FindFirstObjectByType<ObjectiveHud>()?.SetObjective("Clear C4 approach — door locked");
                    Debug.Log("[Mission] Door locked — refuse S5 until DoorUnlocked");
                    return;
                }
            }

            MissionChainController.Instance?.TryAdvance(node);
        }
    }
}
