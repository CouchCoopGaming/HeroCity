using UnityEngine;

namespace HeroCity.Mission
{
    /// <summary>Soft respawn anchors at S0 / S3 / S5 — no full Play reload.</summary>
    public static class SoftCheckpoint
    {
        public static MissionNodeId Node { get; private set; } = MissionNodeId.S0_Boardwalk;
        public static Vector3 Position { get; private set; } = new Vector3(200f, 1f, 54f);
        public static bool HasCheckpoint { get; private set; }

        public static Vector3 DefaultPos(MissionNodeId id) => id switch
        {
            MissionNodeId.S0_Boardwalk => new Vector3(200f, 1f, 54f),
            MissionNodeId.S3_Junction => new Vector3(200f, 1f, 180f),
            MissionNodeId.S5_Hideout => new Vector3(246f, 1f, 240f),
            _ => new Vector3(200f, 1f, 54f)
        };

        public static bool IsSoftNode(MissionNodeId id) =>
            id == MissionNodeId.S0_Boardwalk
            || id == MissionNodeId.S3_Junction
            || id == MissionNodeId.S5_Hideout;

        public static void Register(MissionNodeId id, Vector3? worldPos = null)
        {
            if (!IsSoftNode(id)) return;
            Node = id;
            Position = worldPos ?? DefaultPos(id);
            HasCheckpoint = true;
            Debug.Log($"[SoftCK] Registered {id} @ {Position}");
        }
    }
}
