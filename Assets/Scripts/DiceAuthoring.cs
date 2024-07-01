using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
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
                AddComponent(entity, new PhysicsMassOverride
                {
                    IsKinematic = 1
                });
                AddComponent(entity, new RigidbodyConstraint());
            }
        }
    }

    public struct Dice : IComponentData
    {
        public bool Rolling;
        public bool Falling;
        public float3 Center;
        public float3 Pivot;
        public float3 Axis;
        public float Angle;
        public float RotationSpeed;
        public RollingDirection RollingDirection;
    }

    public enum RollingDirection
    {
        NONE, UP, DOWN, LEFT, RIGHT
    }
}