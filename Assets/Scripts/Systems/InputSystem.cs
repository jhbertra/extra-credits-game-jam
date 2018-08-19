using Unity.Entities;
using UnityEngine;
using Scripts.Components;

namespace Scripts.Systems
{
    public class InputSystem : ComponentSystem
    {

        private struct Components
        {
            public InputComponent Input;
        }

        protected override void OnUpdate()
        {
            foreach (var entity in this.GetEntities<Components>())
            {
                entity.Input.Horizontal = Input.GetAxis(Constants.HorizontalAxis);
                entity.Input.Vertical = Input.GetAxis(Constants.VerticalAxis);
                entity.Input.IsJump = Input.GetButtonDown(Constants.JumpButton);
                entity.Input.IsJumpHeld = Input.GetButton(Constants.JumpButton);
                // entity.Input.IsPush = Input.GetButton(Constants.PushKey);
                // entity.Input.IsPull = Input.GetButton(Constants.PullKey);
            }
        }

    }
}
