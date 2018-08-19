using UnityEngine;

namespace UnitySampleAssets._2D
{

    [RequireComponent(typeof (PlatformerCharacter2D))]
    public class Platformer2DUserControl : MonoBehaviour
    {
        private PlatformerCharacter2D _character;
        private bool _jump;
        private bool _jumpHold;
        private bool _push;
        private bool _pushHold;
        private bool _pull;

        private void Awake()
        {
            this._character = this.GetComponent<PlatformerCharacter2D>();
        }

        private void Update()
        {
            if(!this._jump)
            // Read the jump input in Update so button presses aren't missed.
            this._jump = Input.GetButtonDown("Jump");
            this._jumpHold = Input.GetButton("Jump");
            this._pushHold = Input.GetButton("Push");
            this._push = Input.GetButtonDown("Push");
            this._pull = Input.GetButton("Pull");
        }

        private void FixedUpdate()
        {
            // Read the inputs.
            bool crouch = Input.GetKey(KeyCode.LeftControl);
            float h = Input.GetAxis("Horizontal");
            // Pass all parameters to the character control script.
            this._character.Move(
                h,
                crouch,
                this._jump,
                this._jumpHold,
                this._push,
                this._pushHold,
                this._pull);
            this._jump = false;
        }
    }
}
