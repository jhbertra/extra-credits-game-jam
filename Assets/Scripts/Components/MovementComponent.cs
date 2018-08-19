using UnityEngine;

namespace Scripts.Components
{
    public class MovementComponent : MonoBehaviour
    {
        public bool FacingRight;
        public float MaxSpeed;
        public float JumpForce;
        public float JumpReleaseMultiplier;
        public float BaseGravityScale;
    }
}
