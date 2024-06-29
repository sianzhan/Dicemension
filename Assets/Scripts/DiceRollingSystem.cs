using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine.UIElements;

namespace Dicemension
{
    partial struct DiceRollingSystem : ISystem
    {
        public static void RotateAround(RefRW<LocalTransform> transform, float3 center, float3 pivot, float3 axis, float angle)
        {
            center = transform.ValueRW.Position;
            angle = math.radians(angle);
            var rotation = quaternion.AxisAngle(axis, angle);

            transform.ValueRW.Position = math.mul(rotation, center - pivot) + pivot;
            transform.ValueRW.Rotation = math.mul(rotation, transform.ValueRO.Rotation);
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var input = SystemAPI.GetSingleton<PlayerInput>();

            var rollingDirection = GetRollingDirection(input.Move);
            var (pivot, axis) = GetRollingPivotAxis(rollingDirection);

            foreach (var (dice, transform) in SystemAPI.Query<RefRW<Dice>, RefRW<LocalTransform>>())
            {
                if (dice.ValueRO.Rolling)
                {
                    var deltaAngle = SystemAPI.Time.DeltaTime * dice.ValueRO.RotationSpeed;
                    dice.ValueRW.Angle += deltaAngle;

                    if (dice.ValueRO.Angle >= 90f)
                    {
                        deltaAngle = dice.ValueRO.Angle - 90f;
                        dice.ValueRW.Rolling = false;
                    }

                    RotateAround(transform, dice.ValueRO.Center, dice.ValueRO.Pivot, dice.ValueRO.Axis, deltaAngle);

                    if (!dice.ValueRO.Rolling)
                    {
                        SnapToGrid(transform);
                    }
                }
                else
                {
                    if (rollingDirection == RollingDirection.NONE) continue;

                    var targetPosition = CalculateTargetPosition(rollingDirection, transform.ValueRO.Position);
                    if (!CanMove(targetPosition)) continue;

                    dice.ValueRW.Center = transform.ValueRO.Position;
                    dice.ValueRW.Pivot = transform.ValueRO.Position + pivot;
                    dice.ValueRW.Axis = axis;
                    dice.ValueRW.Angle = 0f;

                    dice.ValueRW.Rolling = true;
                }
            }
        }

        private enum RollingDirection
        {
            NONE, UP, DOWN, LEFT, RIGHT
        }

        [BurstCompile]
        private RollingDirection GetRollingDirection(float2 vec)
        {
            if (vec.y > 0) return RollingDirection.UP;
            if (vec.y < 0) return RollingDirection.DOWN;
            if (vec.x < 0) return RollingDirection.LEFT;
            if (vec.x > 0) return RollingDirection.RIGHT;

            return RollingDirection.NONE;
        }

        [BurstCompile]
        private (float3, float3) GetRollingPivotAxis(RollingDirection rollingDirection)
        {
            var pivot = new float3(0, -0.5f, 0);
            var axis = float3.zero;

            switch (rollingDirection)
            {
                case RollingDirection.UP:
                    pivot.z = 0.5f;
                    axis.x = 1f;
                    break;
                case RollingDirection.DOWN:
                    pivot.z = -0.5f;
                    axis.x = -1f;
                    break;
                case RollingDirection.LEFT:
                    pivot.x = -0.5f;
                    axis.z = 1f;
                    break;
                case RollingDirection.RIGHT:
                    pivot.x = 0.5f;
                    axis.z = -1f;
                    break;
            }

            return (pivot, axis);
        }

        [BurstCompile]
        private float3 CalculateTargetPosition(RollingDirection direction, float3 currentPosition)
        {
            var offset = float3.zero;

            switch (direction)
            {
                case RollingDirection.UP:
                    offset.z = 1f;
                    break;
                case RollingDirection.DOWN:
                    offset.z = -1f;
                    break;
                case RollingDirection.LEFT:
                    offset.x = -1f;
                    break;
                case RollingDirection.RIGHT:
                    offset.x = 1f;
                    break;
            }

            return currentPosition + offset;
        }

        [BurstCompile]
        private bool HasObstacle(float3 position)
        {
            var input = new RaycastInput()
            {
                Start = position + math.up(),
                End = position + math.down(),
                Filter = new CollisionFilter()
                {
                    BelongsTo = PhysicsSettings.Layers.DiceLayer,
                    CollidesWith = PhysicsSettings.Layers.DiceLayer,
                    GroupIndex = 0
                }
            };

            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            bool hit = collisionWorld.CastRay(input);

            if (hit) return true;
            return false;
        }

        [BurstCompile]
        private bool HasGround(float3 position)
        {
            var input = new RaycastInput()
            {
                Start = position,
                End = position + math.down(),
                Filter = new CollisionFilter()
                {
                    BelongsTo = PhysicsSettings.Layers.DiceLayer,
                    CollidesWith = PhysicsSettings.Layers.GroundLayer,
                    GroupIndex = 0
                }
            };

            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            bool hit = collisionWorld.CastRay(input);

            if (hit) return true;
            return false;
        }

        [BurstCompile]
        private bool CanMove(float3 position)
        {
            if (HasObstacle(position)) return false;
            if (HasGround(position)) return true;
            return false;
        }

        [BurstCompile]
        private void SnapToGrid(RefRW<LocalTransform> transform)
        {
            transform.ValueRW.Position = math.round(transform.ValueRO.Position);

            var eulerAngles = math.Euler(transform.ValueRO.Rotation);
            eulerAngles = math.degrees(eulerAngles);
            eulerAngles = math.round(eulerAngles / 90f) * 90f;
            eulerAngles = math.radians(eulerAngles);

            transform.ValueRW.Rotation = quaternion.Euler(eulerAngles);
        }
    }
}