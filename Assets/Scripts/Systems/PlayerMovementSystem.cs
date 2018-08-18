using Scripts.Components;
using Unity.Entities;
using UnityEngine;

namespace Scripts.Systems
{
    public class PlayerMovementSystem : ComponentSystem
    {
        private struct Entity
        {
            public Transform Transform;
            public InputComponent Input;
            public SpeedComponent Speed;
        }

        protected override void OnUpdate()
        {
            foreach (var entity in this.GetEntities<Entity>())
            {
                PlayerMovementSystem.UpdateSpeed(entity);
                PlayerMovementSystem.UpdatePosition(entity);
            }
        }

        private static void UpdateSpeed(Entity entity)
        {
            entity.Speed.HorizontalSpeed = entity.Input.Horizontal;
        }

        private static void UpdatePosition(Entity entity)
        {
            var position = entity.Transform.position;
            var scale = entity.Transform.localScale;

            position.x += entity.Speed.HorizontalSpeed * Time.deltaTime;
            scale.x = entity.Speed.HorizontalSpeed > 0
                ? 1
                : entity.Speed.HorizontalSpeed < 0
                    ? -1
                    : scale.x;
        }
    }
}
