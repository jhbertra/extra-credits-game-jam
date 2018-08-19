using UnityEngine;

namespace UnitySampleAssets._2D
{

    [RequireComponent(typeof (PlatformerCharacter2D))]
    public class Platformer2DUserControl : MonoBehaviour
    {
        private PlatformerCharacter2D character;
        private bool jump;
        private bool jumpHold;
        private bool push;
        private bool pushHold;
        private bool pull;
        private bool pullHold;

        private void Awake()
        {
            character = GetComponent<PlatformerCharacter2D>();
        }

        private void Update()
        {
            if(!jump)
            // Read the jump input in Update so button presses aren't missed.
            this.jump = Input.GetButtonDown("Jump");
            this.jumpHold = Input.GetButton("Jump");
            this.push = Input.GetButtonDown("Push");
            this.pushHold = Input.GetButton("Push");
            this.pull = Input.GetButtonDown("Pull");
            this.pullHold = Input.GetButton("Pull");
        }

        private void FixedUpdate()
        {
            // Read the inputs.
            bool crouch = Input.GetKey(KeyCode.LeftControl);
            float h = Input.GetAxis("Horizontal");
            // Pass all parameters to the character control script.
            character.Move(h, crouch, jump, this.jumpHold);
            jump = false;
        }
    }
}
