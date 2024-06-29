using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Dicemension
{
    public class DiceAuthoring : MonoBehaviour
    {
        [Tooltip("Degrees Angle Per Second")]
        public float RotationSpeed = 90f;

        public class Baker : Baker<DiceAuthoring>
        {
            public override void Bake(DiceAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Dice
                {
                    RotationSpeed = authoring.RotationSpeed
                });
            }
        }
    }

    public struct Dice : IComponentData
    {
        public bool Rolling;
        public float3 Center;
        public float3 Pivot;
        public float3 Axis;
        public float Angle;
        public float RotationSpeed;
    }
}