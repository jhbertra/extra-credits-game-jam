using Scripts.Components;
using Unity.Entities;
using UnityEngine;

namespace Scripts.Systems
{
    public class HorizontalMovementSystem : ComponentSystem
    {
        private struct Entity
        {
            public Animator Animator;
            public InputComponent Input;
            public MovementComponent Movement;
            public Rigidbody2D Rigidbody2D;
            public Transform Transform;
        }

        protected override void OnUpdate()
        {
            foreach (var entity in this.GetEntities<Entity>())
            {
                // The Speed animator parameter is set to the absolute value of the horizontal input.
                entity.Animator.SetFloat("Speed", Mathf.Abs(entity.Input.Horizontal));

                // Move the character
                entity.Rigidbody2D.velocity = new Vector2(
                    entity.Input.Horizontal * entity.Movement.MaxSpeed,
                    entity.Rigidbody2D.velocity.y);

                // If the input is moving the player right and the player is facing left...
                if (entity.Input.Horizontal > 0 && !entity.Movement.FacingRight
                    || entity.Input.Horizontal < 0 && entity.Movement.FacingRight)
                {
                    // Switch the way the player is labelled as facing.
                    entity.Movement.FacingRight = !entity.Movement.FacingRight;

                    // Multiply the player's x local scale by -1.
                    var theScale = entity.Transform.localScale;
                    theScale.x *= -1;
                    entity.Transform.localScale = theScale;
                }
            }
        }
    }
}
