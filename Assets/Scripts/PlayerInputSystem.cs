using Unity.Burst;
using Unity.Entities;
using UnityEngine.InputSystem;
using UnityEngine;

namespace Dicemension
{
    [BurstCompile]
    partial class PlayerInputSystem : SystemBase
    {
        InputAction moveAction;

        [BurstCompile]
        protected override void OnCreate()
        {
            moveAction = InputSystem.actions.FindAction("Move");

            EntityManager.AddComponent<PlayerInput>(SystemHandle);
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            var playerInput = SystemAPI.GetSingletonRW<PlayerInput>();

            playerInput.ValueRW.Move = moveAction.ReadValue<Vector2>();
        }
    }
}