using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Unity.Mathematics;
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
        /// The rate of positive change of horizonal speed
        /// </summary>
        [FormerlySerializedAs("maxSpeed")] [SerializeField]
        private float _positiveAcceleration = 10f;

        /// <summary>
        /// The rate of positive change of horizonal speed
        /// </summary>
        [FormerlySerializedAs("maxSpeed")] [SerializeField]
        private float _negativeAcceleration = 10f;

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
        [FormerlySerializedAs("_jumpReleaseDecrement")] [FormerlySerializedAs("jumpReleaseDamping")] [SerializeField]
        private float _jumpReleaseDamping = 1f;

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
        private int _magnetRange = 10;

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
        private const float MetalRadius = .05f;
        private const float CeilingRadius = .01f;


        /*
         * Initial State
         */

        // ReSharper disable NotNullMemberIsNotInitialized
        [NotNull] private Transform _groundCheck; // A position marking where to check if the player is grounded.
        [NotNull] private Transform _ceilingCheck; // A position marking where to check for ceilings
        [NotNull] private Transform _floorMetalCheck; // A position marking where to check for underfoot metal
        [NotNull] private Transform _rearMetalCheck; // A position marking where to check for metal behind
        [NotNull] private Transform _frontMetalCheck; // A position marking where to check for metal in front
        [NotNull] private Transform _arm; // The transform of the arm
        [NotNull] private Animator _anim; // Reference to the player's animator component.
        [NotNull] private Rigidbody2D _rigidBody2D;
        // ReSharper restore NotNullMemberIsNotInitialized
        private float _baseGravityScale;


        /*
         * Mutable State
         */

        private bool _facingRight = true;
        private bool _grounded;
        private bool _isMetalUnderfoot;
        private bool _isMetalInFront;
        private bool _isMetalBehind;
        private bool _isMetalAbove;
        [CanBeNull] private Collider2D _closestMetalSource;
        [CanBeNull] private Collider2D _activeMetal;
        private MagnetAction _magnetAction;


        /*
         * Engine Callbacks
         */

        private void Awake()
        {
            // Setting up references.
            this._groundCheck = this.transform.Find("GroundCheck");
            this._ceilingCheck = this.transform.Find("CeilingCheck");
            this._floorMetalCheck = this.transform.Find("FloorMetalCheck");
            this._frontMetalCheck = this.transform.Find("FrontMetalCheck");
            this._rearMetalCheck = this.transform.Find("RearMetalCheck");
            this._arm = this.transform.Find("Arm");
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
            this._isMetalUnderfoot = Physics2D.OverlapCircle(
                this._floorMetalCheck.position,
                PlatformerCharacter2D.MetalRadius,
                this._whatIsMetal);
            this._isMetalInFront = Physics2D.OverlapCircle(
                this._frontMetalCheck.position,
                PlatformerCharacter2D.MetalRadius,
                this._whatIsMetal);
            this._isMetalBehind = Physics2D.OverlapCircle(
                this._rearMetalCheck.position,
                PlatformerCharacter2D.MetalRadius,
                this._whatIsMetal);
            this._isMetalAbove = Physics2D.OverlapCircle(
                this._ceilingCheck.position,
                PlatformerCharacter2D.MetalRadius,
                this._whatIsMetal);
            this._anim.SetBool("Ground", this._grounded);

            // Set the vertical animation
            this._anim.SetFloat("vSpeed", this._rigidBody2D.velocity.y);
        }


        public void SetArmX(float x)
        {
            this._arm.localPosition = new Vector2(x, this._arm.localPosition.y);
        }


        public void SetArmY(float y)
        {
            this._arm.localPosition = new Vector2(this._arm.localPosition.x, y);
        }


        /*
         * Public interface
         */

        public void Move(
            float horizontal,
            float vertical,
            bool crouch,
            bool jump,
            bool jumpHold,
            bool push,
            bool pushHold,
            bool pull)
        {
            var debug = new StringWriter();
            var forces = new List<Vector2>();

            this._closestMetalSource =
                PlatformerCharacter2D.DetectMetal(
                    this.transform.position,
                    this._magnetRange,
                    this._whatIsMetal);

            var targetVelocity = this._maxSpeed * horizontal;

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
                horizontal = (crouch ? horizontal * this._crouchSpeed : horizontal);

                var baseAcceleration = Vector2.right * (targetVelocity - this._rigidBody2D.velocity.x);
                var acceleration = Math.Sign(baseAcceleration.x) != Math.Sign(this._rigidBody2D.velocity.x)
                    ? this._negativeAcceleration
                    : this._positiveAcceleration;
                forces.Add(baseAcceleration * acceleration);

                // Move the character
                //this._rigidBody2D.velocity = new Vector2(move * this._maxSpeed, this._rigidBody2D.velocity.y);

                // If the input is moving the player right and the player is facing left...
                if (horizontal > 0 && !this._facingRight
                    || horizontal < 0 && this._facingRight)

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
                forces.Add(Vector2.up * this._jumpForce);
            }

            if (!this._grounded && !jumpHold && !pushHold && this._rigidBody2D.velocity.y > 0)
            {
                forces.Add(Vector2.down * this._jumpReleaseDamping);
            }

            this._rigidBody2D.gravityScale = this._rigidBody2D.velocity.y <= 0f
                ? this._baseGravityScale * this._fallMultiplier
                : this._baseGravityScale;

            if (pushHold || pull)
            {
                if (this._magnetAction == MagnetAction.Nothing)
                {
                    this._magnetAction = pushHold
                        ? MagnetAction.Push
                        : MagnetAction.Pull;
                    this._activeMetal = this._closestMetalSource;
                }
            }
            else
            {
                this._magnetAction = MagnetAction.Nothing;
                this._activeMetal = null;
            }

            switch (this._magnetAction)
            {
                case MagnetAction.Pull when this._activeMetal != null:
                    if (this._isMetalUnderfoot || this._isMetalAbove || this._isMetalInFront || this._isMetalBehind)
                    {
                        this._rigidBody2D.velocity = Vector2.zero;
                        this._rigidBody2D.gravityScale = 0f;
                        forces.Clear();
                    }
                    else
                    {
                        forces.Add(
                            PlatformerCharacter2D.GetPullForce(
                                this._activeMetal.transform.position,
                                this.transform.position,
                                this._magnetForce));
                    }
                    break;

                case MagnetAction.Push:
                    forces.Add(
                        PlatformerCharacter2D.GetInitialPushForce(
                            this._activeMetal,
                            this.transform.position,
                            this._magnetForce));
                    this._grounded = false;
                    this._anim.SetBool("Ground", false);
                    break;
            }

            var armPos = this._arm.position;
            var sign = this._facingRight ? 1 : -1;

            var armTarget =
                (this._activeMetal != null
                    ? (Vector2) (this._activeMetal.transform.position - armPos)
                    : math.abs(vertical) < float.Epsilon
                        ? Vector2.right * sign
                        : new Vector2(horizontal, vertical))
                .normalized;

            var armAngle = math.atan2(1f * sign, 0f) - math.atan2(armTarget.x, armTarget.y);
            this._arm.transform.rotation = Quaternion.Euler(Vector3.forward * math.degrees(armAngle));

            var force = forces.Aggregate(Vector2.zero, (x, y) => x + y);
            var effectiveForce = force + (Vector2)Physics.gravity;
            debug.WriteLine($"ActiveMetal: {this._activeMetal?.GetInstanceID()}");
            debug.WriteLine($"IsMetalUnderfoot: {this._isMetalUnderfoot.ToString()}");
            debug.WriteLine($"IsMetalInFront: {this._isMetalInFront.ToString()}");
            debug.WriteLine($"IsMetalAbove: {this._isMetalAbove.ToString()}");
            Debug.DrawRay(this.transform.position, effectiveForce, Color.green);

            // The Speed animator parameter is set to the absolute value of the horizontal input.
            this._anim.SetFloat("Speed", Mathf.Abs(this._rigidBody2D.velocity.x));
            this._rigidBody2D.AddForce(force);
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

        private static Vector2 GetContinuousPushForce(
            [NotNull] Collider2D metalSource,
            Vector3 playerPosition,
            float force)
        {
            var direction = PlatformerCharacter2D.GetInitialPushForce(metalSource, playerPosition, force).normalized;
            var distance = math.abs(Vector2.Distance(playerPosition, metalSource.transform.position));
            var effectiveForce = 1f / math.sqrt(distance) * force;
            return direction * effectiveForce * 0.01f;
        }

        private static Vector2 GetInitialPushForce(
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

        private static Vector2 GetPullForce(
            Vector2 metalSourcePosition,
            Vector2 playerPosition,
            float force)
        {
            var effectVector = (metalSourcePosition - playerPosition).normalized;
            return effectVector * force;
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
