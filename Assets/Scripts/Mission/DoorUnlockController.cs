using UnityEngine;

namespace HeroCity.Mission
{
    /// <summary>Disables the physical hideout door blocker once DoorUnlocked is true.</summary>
    public class DoorUnlockController : MonoBehaviour
    {
        bool _cleared;

        void Update()
        {
            if (_cleared) return;
            var chain = MissionChainController.Instance;
            if (chain == null || !chain.DoorUnlocked) return;
            _cleared = true;
            Debug.Log("[Mission] Hideout_Door_Blocker cleared (DoorUnlocked)");
            gameObject.SetActive(false);
        }
    }
}
