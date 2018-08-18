using Scripts.Components;
using Unity.Entities;
using UnityEngine;

namespace Scripts.Systems
{
    public class PlayerMovementSystem : ComponentSystem
    {
        private struct Components
        {
            public Transform Transform;
            public InputComponent Input;
            public SpeedComponent Speed;
        }

        protected override void OnUpdate()
        {
            foreach (var entity in this.GetEntities<Components>())
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
}
