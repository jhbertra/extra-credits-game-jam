using UnityEngine;
using UnityEngine.Networking;

namespace UnitySampleAssets._2D
{

    public class PlatformerCharacter2D : MonoBehaviour
    {
        private bool facingRight = true; // For determining which way the player is currently facing.

        [SerializeField] private float maxSpeed = 10f; // The fastest the player can travel in the x axis.
        [SerializeField] private float jumpForce = 400f; // Amount of force added when the player jumps.
        [SerializeField] private float jumpReleaseDamping = 400f; // Amount of force added when the player jumps.
        [SerializeField] private float fallMultiplier = 1f; // Amount of force added when the player jumps.

        [Range(0, 1)] [SerializeField] private float crouchSpeed = .36f;
                                                     // Amount of maxSpeed applied to crouching movement. 1 = 100%

        [SerializeField] private bool airControl = false; // Whether or not a player can steer while jumping;
        [SerializeField] private LayerMask whatIsGround; // A mask determining what is ground to the character

        private Transform groundCheck; // A position marking where to check if the player is grounded.
        private float groundedRadius = .2f; // Radius of the overlap circle to determine if grounded
        private bool grounded = false; // Whether or not the player is grounded.
        private Transform ceilingCheck; // A position marking where to check for ceilings
        private float ceilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up
        private Animator anim; // Reference to the player's animator component.
        private Rigidbody2D _rigidBody2d;
        private float baseGravityScale;


        private void Awake()
        {
            // Setting up references.
            this.groundCheck = this.transform.Find("GroundCheck");
            this.ceilingCheck = this.transform.Find("CeilingCheck");
            this.anim = this.GetComponent<Animator>();
            this._rigidBody2d = this.GetComponent<Rigidbody2D>();
            this.baseGravityScale = this._rigidBody2d.gravityScale;
        }


        private void FixedUpdate()
        {
            // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
            this.grounded = Physics2D.OverlapCircle(this.groundCheck.position, this.groundedRadius, this.whatIsGround);
            this.anim.SetBool("Ground", this.grounded);

            // Set the vertical animation
            this.anim.SetFloat("vSpeed", this._rigidBody2d.velocity.y);
        }


        public void Move(float move, bool crouch, bool jump, bool jumpHold)
        {


            // If crouching, check to see if the character can stand up
            if (!crouch && this.anim.GetBool("Crouch"))
            {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                if (Physics2D.OverlapCircle(this.ceilingCheck.position, this.ceilingRadius, this.whatIsGround))
                    crouch = true;
            }

            // Set whether or not the character is crouching in the animator
            this.anim.SetBool("Crouch", crouch);

            //only control the player if grounded or airControl is turned on
            if (this.grounded || this.airControl)
            {
                // Reduce the speed if crouching by the crouchSpeed multiplier
                move = (crouch ? move* this.crouchSpeed : move);

                // The Speed animator parameter is set to the absolute value of the horizontal input.
                this.anim.SetFloat("Speed", Mathf.Abs(move));

                // Move the character
                this._rigidBody2d.velocity = new Vector2(move* this.maxSpeed, this._rigidBody2d.velocity.y);

                // If the input is moving the player right and the player is facing left...
                if (move > 0 && !this.facingRight)
                    // ... flip the player.
                    this.Flip();
                    // Otherwise if the input is moving the player left and the player is facing right...
                else if (move < 0 && this.facingRight)
                    // ... flip the player.
                    this.Flip();
            }
            // If the player should jump...
            if (this.grounded && jump && this.anim.GetBool("Ground"))
            {
                // Add a vertical force to the player.
                this.grounded = false;
                this.anim.SetBool("Ground", false);
                this._rigidBody2d.AddForce(new Vector2(0f, this.jumpForce));
            }

            if (!this.grounded && !jumpHold && this._rigidBody2d.velocity.y > 0)
            {
                this._rigidBody2d.velocity = new Vector2(this._rigidBody2d.velocity.x, this._rigidBody2d.velocity.y / this.jumpReleaseDamping);
            }

            if (this._rigidBody2d.velocity.y <= 0f)
            {
                this._rigidBody2d.gravityScale = this.baseGravityScale * this.fallMultiplier;
            }
            else
            {
                this._rigidBody2d.gravityScale = this.baseGravityScale;
            }
        }


        private void Flip()
        {
            // Switch the way the player is labelled as facing.
            this.facingRight = !this.facingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = this.transform.localScale;
            theScale.x *= -1;
            this.transform.localScale = theScale;
        }
    }
}
