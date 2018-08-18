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
                entity.Input.IsJump = Input.GetKey(Constants.JumpKey);
                entity.Input.IsPush = Input.GetKey(Constants.PushKey);
                entity.Input.IsPull = Input.GetKey(Constants.PullKey);
            }
        }

    }
}
