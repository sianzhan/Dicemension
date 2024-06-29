using Unity.Entities;
using Unity.Mathematics;

namespace Dicemension
{
    public struct PlayerInput : IComponentData
    {
        public float2 Move;
    }
}