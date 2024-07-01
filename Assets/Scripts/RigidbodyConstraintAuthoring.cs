using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

namespace Dicemension
{
    // This system is intended as a workaround for the rigidbody constraint of the Unity Physics which is not working.
    public class RigidbodyConstraintAuthoring : MonoBehaviour
    {
        public bool3 FreezePosition;
        public bool3 FreezeRotation;

        public class Baker : Baker<RigidbodyConstraintAuthoring>
        {
            public override void Bake(RigidbodyConstraintAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new RigidbodyConstraint
                {
                    FreezePosition = authoring.FreezePosition,
                    FreezeRotation = authoring.FreezeRotation,
                    PositionCache = authoring.transform.position,
                    RotationCache = authoring.transform.rotation
                });
            }
        }
    }

    public struct RigidbodyConstraint : IComponentData
    {
        public bool3 FreezePosition;
        public bool3 FreezeRotation;
        public float3 PositionCache;
        public quaternion RotationCache;
    }
}