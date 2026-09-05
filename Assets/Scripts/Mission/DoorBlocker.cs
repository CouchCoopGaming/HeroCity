using UnityEngine;

namespace HeroCity.Mission
{
    /// <summary>Solid hideout door gap blocker — disables when DoorUnlocked.</summary>
    public class DoorBlocker : MonoBehaviour
    {
        bool _cleared;

        void Update()
        {
            if (_cleared) return;
            var chain = MissionChainController.Instance;
            if (chain == null || !chain.DoorUnlocked) return;
            _cleared = true;
            Debug.Log("[Mission] DoorBlocker cleared (DoorUnlocked)");
            gameObject.SetActive(false);
        }
    }
}
