using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

// This system is intended as a workaround for the rigidbody constraint of the Unity Physics which is not working.

namespace Dicemension
{
    [UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
    partial struct RigidbodyConstraintTransformCacheSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (constraint, transform)
            in SystemAPI.Query<RefRW<RigidbodyConstraint>, RefRO<LocalTransform>>())
            {
                constraint.ValueRW.PositionCache = transform.ValueRO.Position;
                constraint.ValueRW.RotationCache = transform.ValueRO.Rotation;
            }
        }
    }

    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    partial struct RigidbodyConstraintSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (constraint, velocity, transform)
            in SystemAPI.Query<RefRO<RigidbodyConstraint>, RefRW<PhysicsVelocity>, RefRW<LocalTransform>>())
            {
                var deltaRotation = math.mul(transform.ValueRW.Rotation, math.inverse(constraint.ValueRO.RotationCache));

                for (var i = 0; i < 3; ++i)
                {
                    if (constraint.ValueRO.FreezePosition[i])
                    {
                        velocity.ValueRW.Linear[i] = 0;
                        transform.ValueRW.Position[i] = constraint.ValueRO.PositionCache[i];
                    }

                    if (constraint.ValueRO.FreezeRotation[i])
                    {
                        velocity.ValueRW.Angular[i] = 0;
                        deltaRotation.value[i] = 0;
                    }
                }

                transform.ValueRW.Rotation = math.mul(deltaRotation, constraint.ValueRO.RotationCache);
            }
        }
    }
}