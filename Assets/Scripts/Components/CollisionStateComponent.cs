using UnityEngine;

namespace Scripts.Components
{
    public class CollisionStateComponent : MonoBehaviour
    {
        public bool IsGrounded;
        public LayerMask WhatIsGround;
    }
}
