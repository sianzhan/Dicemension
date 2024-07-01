using UnityEngine;

namespace Dicemension
{
    public static class PhysicsSettings
    {
        public static class Layers
        {
            public readonly static uint DiceLayer = 1u << 9;
            public readonly static uint GroundLayer = 1u << 10;
            public readonly static uint HoleLayer = 1u << 11;
        }
    }
}