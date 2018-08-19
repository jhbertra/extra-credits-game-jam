using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UnitySampleAssets._2D
{
    public class PlatformerCharacter2D : MonoBehaviour
    {
        /*
         * Parameters
         */

        /// <summary>
        /// A handle to the UI element that displays debug text
        /// </summary>
        [SerializeField] [CanBeNull] private Text _debug;

        /// <summary>
        /// The maximum running speed
        /// </summary>
        [FormerlySerializedAs("maxSpeed")] [SerializeField]
        private float _maxSpeed = 10f;

        /// <summary>
        /// The force applied when jumping
        /// </summary>
        [FormerlySerializedAs("jumpForce")] [SerializeField]
        private float _jumpForce = 400f;

        /// <summary>
        /// The force applied when pulling on the magnet
        /// </summary>
        [FormerlySerializedAs("jumpForce")] [SerializeField]
        private float _magnetForce = 400f;

        /// <summary>
        /// The amount to decrement the vertical speed when ascending and not holding jump
        /// </summary>
        [FormerlySerializedAs("jumpReleaseDamping")] [SerializeField]
        private float _jumpReleaseDecrement = 1f;

        /// <summary>
        /// The amount to increase gravity by when falling
        /// </summary>
        [FormerlySerializedAs("fallMultiplier")] [SerializeField]
        private float _fallMultiplier = 1f;

        /// <summary>
        /// The Minimum speed to travel when pulling
        /// </summary>
        [SerializeField] private float _minPullSpeed;

        /// <summary>
        /// The range of the magnetic effect
        /// </summary>
        [FormerlySerializedAs("magnetismRange")] [SerializeField]
        private int _magnetRange = 5;

        /// <summary>
        /// Amount of maxSpeed applied to crouching movement. 1 = 100%
        /// </summary>
        [FormerlySerializedAs("crouchSpeed")] [Range(0, 1)] [SerializeField]
        private float _crouchSpeed = .36f;

        /// <summary>
        /// Whether or not a player can steer while jumping;
        /// </summary>
        [FormerlySerializedAs("airControl")] [SerializeField]
        private bool _airControl;

        /// <summary>
        /// A mask determining what is ground to the character
        /// </summary>
        [FormerlySerializedAs("whatIsGround")] [SerializeField]
        private LayerMask _whatIsGround;

        /// <summary>
        /// A mask determining what is metal to the character
        /// </summary>
        [SerializeField] private LayerMask _whatIsMetal;


        /*
         * Constants
         */

        private const float GroundedRadius = .2f;
        private const float CeilingRadius = .01f;


        /*
         * Initial State
         */

        // ReSharper disable NotNullMemberIsNotInitialized
        [NotNull] private Transform _groundCheck; // A position marking where to check if the player is grounded.
        [NotNull] private Transform _ceilingCheck; // A position marking where to check for ceilings
        [NotNull] private Animator _anim; // Reference to the player's animator component.

        [NotNull] private Rigidbody2D _rigidBody2D;

        // ReSharper restore NotNullMemberIsNotInitialized
        private float _baseGravityScale;


        /*
         * Mutable State
         */

        private bool _facingRight = true;
        private bool _grounded;
        [CanBeNull] private Collider2D _closestMetalSource;
        private MagnetAction _magnetAction;


        /*
         * Engine Callbacks
         */

        private void Awake()
        {
            // Setting up references.
            this._groundCheck = this.transform.Find("GroundCheck");
            this._ceilingCheck = this.transform.Find("CeilingCheck");
            this._anim = this.GetComponent<Animator>();
            this._rigidBody2D = this.GetComponent<Rigidbody2D>();
            this._baseGravityScale = this._rigidBody2D.gravityScale;
        }


        private void FixedUpdate()
        {
            // The player is grounded if a circle-cast to the ground-check position hits anything designated as ground
            this._grounded = Physics2D.OverlapCircle(
                this._groundCheck.position,
                PlatformerCharacter2D.GroundedRadius,
                this._whatIsGround);
            this._anim.SetBool("Ground", this._grounded);

            // Set the vertical animation
            this._anim.SetFloat("vSpeed", this._rigidBody2D.velocity.y);
        }


        /*
         * Public interface
         */

        public void Move(float move, bool crouch, bool jump, bool jumpHold, bool push, bool pushHold, bool pull)
        {
            var debug = new StringWriter();

            this._closestMetalSource =
                PlatformerCharacter2D.DetectMetal(
                    this.transform.position,
                    this._magnetRange,
                    this._whatIsMetal);

            Debug.DrawRay(this.transform.position, Vector2.left * this._magnetRange, Color.red);
            Debug.DrawRay(this.transform.position, Vector2.right * this._magnetRange, Color.red);
            Debug.DrawRay(this.transform.position, Vector2.up * this._magnetRange, Color.red);
            Debug.DrawRay(this.transform.position, Vector2.down * this._magnetRange, Color.red);

            // If crouching, check to see if the character can stand up
            if (!crouch && this._anim.GetBool("Crouch"))
            {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                if (Physics2D.OverlapCircle(this._ceilingCheck.position, PlatformerCharacter2D.CeilingRadius,
                    this._whatIsGround))
                {
                    crouch = true;
                }
            }

            // Set whether or not the character is crouching in the animator
            this._anim.SetBool("Crouch", crouch);

            //only control the player if grounded or airControl is turned on
            if ((this._grounded || this._airControl) && this._magnetAction != MagnetAction.Pull)
            {
                // Reduce the speed if crouching by the crouchSpeed multiplier
                move = (crouch ? move * this._crouchSpeed : move);

                // The Speed animator parameter is set to the absolute value of the horizontal input.
                this._anim.SetFloat("Speed", Mathf.Abs(move));

                // Move the character
                this._rigidBody2D.velocity = new Vector2(move * this._maxSpeed, this._rigidBody2D.velocity.y);

                // If the input is moving the player right and the player is facing left...
                if (move > 0 && !this._facingRight
                    || move < 0 && this._facingRight)

                    // ... flip the player.
                {
                    // Switch the way the player is labelled as facing.
                    this._facingRight = !this._facingRight;

                    // Multiply the player's x local scale by -1.
                    var theScale = this.transform.localScale;
                    theScale.x *= -1;
                    this.transform.localScale = theScale;
                }
            }

            // If the player should jump...
            if (this._grounded && jump && this._anim.GetBool("Ground"))
            {
                // Add a vertical force to the player.
                this._grounded = false;
                this._anim.SetBool("Ground", false);
                this._rigidBody2D.velocity = new Vector2(this._rigidBody2D.velocity.x, 15f);
            }

            if (!this._grounded && !jumpHold && !pushHold && this._rigidBody2D.velocity.y > 0)
            {
                this._rigidBody2D.velocity = new Vector2(this._rigidBody2D.velocity.x,
                    this._rigidBody2D.velocity.y - this._jumpReleaseDecrement);
            }

            this._rigidBody2D.gravityScale = this._rigidBody2D.velocity.y <= 0f
                ? this._baseGravityScale * this._fallMultiplier
                : this._baseGravityScale;

            if (pushHold || pull && this._magnetAction != MagnetAction.Nothing)
            {
                this._magnetAction = pushHold
                    ? MagnetAction.Push
                    : MagnetAction.Pull;
            }
            else
            {
                this._magnetAction = MagnetAction.Nothing;
            }

            switch (this._magnetAction)
            {
                case MagnetAction.Pull when this._closestMetalSource != null:
                    this._rigidBody2D.velocity =
                        PlatformerCharacter2D.GetPullVelocity(
                            this._closestMetalSource.transform.position,
                            this.transform.position,
                            this._magnetForce);
                    break;

                case MagnetAction.Push when this._closestMetalSource != null && push:
                    this._rigidBody2D.velocity =
                        PlatformerCharacter2D.GetInitialPushVelocity(
                            this._closestMetalSource,
                            this.transform.position,
                            15f);
                    this._grounded = false;
                    this._anim.SetBool("Ground", false);

                    break;
            }

            this._debug.text = debug.ToString();
        }


        /*
         * Helper functions
         */

        private static Collider2D DetectMetal(Vector2 playerPosition, float magnetRange, LayerMask whatIsMetal)
        {
            return Physics2D
                .OverlapCircleAll(playerPosition, magnetRange, whatIsMetal)
                .FirstOrDefault();
        }

        private static Vector2 GetInitialPushVelocity(
            [NotNull] Collider2D metalSource,
            Vector2 playerPosition,
            float force)
        {
            var box = metalSource.bounds;
            var forceDir = new Vector2(
                playerPosition.x < box.min.x ? -1f : (playerPosition.x >= box.max.x ? 1f : 0f),
                playerPosition.y > box.min.y ? 1f : (playerPosition.y <= box.max.y ? -1f : 0f));

            return forceDir.normalized * force;
        }

        private static Vector2 GetPullVelocity(
            Vector2 metalSourcePosition,
            Vector2 playerPosition,
            float force)
        {
            var effectVector = (metalSourcePosition - playerPosition).normalized;
            return effectVector * force * Time.deltaTime;
        }


        /*
         * Types
         */

        private enum MagnetAction
        {
            Nothing = 0,
            Push,
            Pull
        }
    }
}
