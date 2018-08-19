using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnitySampleAssets._2D
{
    public class PlatformerCharacter2D : MonoBehaviour
    {
        private bool _facingRight = true; // For determining which way the player is currently facing.

        [FormerlySerializedAs("maxSpeed")] [SerializeField]
        private float _maxSpeed = 10f; // The fastest the player can travel in the x axis.

        [FormerlySerializedAs("jumpForce")] [SerializeField]
        private float _jumpForce = 400f; // Amount of force added when the player jumps.

        [FormerlySerializedAs("jumpForce")] [SerializeField]
        private float _magnetForce = 400f; // Amount of force added when the player engages the magnet.

        [FormerlySerializedAs("jumpReleaseDamping")] [SerializeField]
        private float _jumpReleaseDamping = 400f; // Amount of force added when the player jumps.

        [FormerlySerializedAs("fallMultiplier")] [SerializeField]
        private float _fallMultiplier = 1f; // Amount of force added when the player jumps.

        [SerializeField] private int magnetismRange = 5;
        [SerializeField] private float magnetismSpeed = 1.0f;

        [FormerlySerializedAs("crouchSpeed")] [Range(0, 1)] [SerializeField]
        private float _crouchSpeed = .36f;

        // Amount of maxSpeed applied to crouching movement. 1 = 100%

        [FormerlySerializedAs("airControl")] [SerializeField]
        private bool _airControl; // Whether or not a player can steer while jumping;

        [FormerlySerializedAs("whatIsGround")] [SerializeField]
        private LayerMask _whatIsGround; // A mask determining what is ground to the character

        [FormerlySerializedAs("whatIsGround")] [SerializeField]
        private LayerMask _whatIsMetal; // A mask determining what is ground to the character

        private const float GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded

        private const float
            CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up

        private Transform _groundCheck; // A position marking where to check if the player is grounded.
        private bool _grounded; // Whether or not the player is grounded.
        private Transform _ceilingCheck; // A position marking where to check for ceilings
        private Animator _anim; // Reference to the player's animator component.
        private Rigidbody2D _rigidBody2d;
        private float _baseGravityScale;

        private Transform _closestMetalSource;
        private Transform _affectedMetal;
        private MagnetAction _magnetAction;


        private void Awake()
        {
            // Setting up references.
            this._groundCheck = this.transform.Find("GroundCheck");
            this._ceilingCheck = this.transform.Find("CeilingCheck");
            this._anim = this.GetComponent<Animator>();
            this._rigidBody2d = this.GetComponent<Rigidbody2D>();
            this._baseGravityScale = this._rigidBody2d.gravityScale;
        }


        private void FixedUpdate()
        {
            // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
            this._grounded = Physics2D.OverlapCircle(this._groundCheck.position, PlatformerCharacter2D.GroundedRadius,
                this._whatIsGround);
            this._anim.SetBool("Ground", this._grounded);

            // Set the vertical animation
            this._anim.SetFloat("vSpeed", this._rigidBody2d.velocity.y);
        }


        public void Move(
            float move,
            bool crouch,
            bool jump,
            bool jumpHold,
            bool push,
            bool pull)
        {
            this._closestMetalSource = this.DetectMetal();

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
            if (this._grounded || this._airControl)
            {
                // Reduce the speed if crouching by the crouchSpeed multiplier
                move = (crouch ? move * this._crouchSpeed : move);

                // The Speed animator parameter is set to the absolute value of the horizontal input.
                this._anim.SetFloat("Speed", Mathf.Abs(move));

                // Move the character
                this._rigidBody2d.velocity = new Vector2(move * this._maxSpeed, this._rigidBody2d.velocity.y);

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
                this._rigidBody2d.AddForce(new Vector2(0f, this._jumpForce));
            }

            if (!this._grounded && !jumpHold && this._rigidBody2d.velocity.y > 0)
            {
                this._rigidBody2d.velocity = new Vector2(this._rigidBody2d.velocity.x,
                    this._rigidBody2d.velocity.y / this._jumpReleaseDamping);
            }

            if (this._rigidBody2d.velocity.y <= 0f)
            {
                this._rigidBody2d.gravityScale = this._baseGravityScale * this._fallMultiplier;
            }
            else
            {
                this._rigidBody2d.gravityScale = this._baseGravityScale;
            }

            if (push || pull && this._magnetAction == MagnetAction.Nothing)
            {
                this._affectedMetal = this._closestMetalSource;
                this._magnetAction = push
                    ? MagnetAction.Push
                    : MagnetAction.Pull;
            }
            else if (!push && !pull)
            {
                this._affectedMetal = null;
                this._magnetAction = MagnetAction.Nothing;
            }

            if (this._magnetAction == MagnetAction.Push || this._magnetAction == MagnetAction.Pull)
            {
                var affectVector = (this._affectedMetal.position - this.transform.position).normalized;

                Debug.DrawRay(this.transform.position, affectVector, Color.cyan);

                if (this._magnetAction == MagnetAction.Push)
                {
                    this._rigidBody2d.AddForce(
                        affectVector
                        * -math.max(
                            0f,
                            math.lerp(
                                this._magnetForce,
                                0f,
                                Vector2.Distance(
                                    this.transform.position,
                                    this._affectedMetal.position)
                                / this.magnetismRange)));
                }
            }
        }

        private Transform DetectMetal()
        {
            return Physics2D
                .OverlapCircleAll(transform.position, magnetismRange, this._whatIsMetal)
                .Select(x => x.transform)
                .FirstOrDefault();
        }

        enum MagnetAction
        {
            Nothing = 0,
            Push,
            Pull
        }
    }

}
