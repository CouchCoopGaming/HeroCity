using UnityEngine;

namespace HeroCity.Core
{
    /// <summary>Ensures GameFlow exists on Boot.</summary>
    public class BootStrap : MonoBehaviour
    {
        void Awake()
        {
            if (GameFlow.Instance == null)
            {
                var go = new GameObject("GameFlow");
                go.AddComponent<GameFlow>();
            }
        }
    }
}
