using UnityEngine;

namespace SadnessMonday.BetterPhysics {
    public static class Rigidbody2DExtensions {
        internal static Vector2 AddLinearVelocity(this Rigidbody2D rb, in Vector2 addend) {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity += addend;
#else
            return rb.velocity += addend;
#endif
        }
        
        internal static Vector2 GetLinearVelocity(this Rigidbody2D rb) {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }
        
        internal static Vector2 SetLinearVelocity(this Rigidbody2D rb, Vector2 velocity) {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity = velocity;
#else
            return rb.velocity = velocity;
#endif
        }
        
        internal static float GetLinearDamping(this Rigidbody2D rb) {
#if UNITY_6000_0_OR_NEWER
            return rb.linearDamping;
#else
            return rb.drag;
#endif
        }
        
        internal static float SetLinearDamping(this Rigidbody2D rb, float damping) {
#if UNITY_6000_0_OR_NEWER
            return rb.linearDamping = damping;
#else
            return rb.drag = damping;
#endif
        }
        
        internal static float GetAngularDamping(this Rigidbody2D rb) {
#if UNITY_6000_0_OR_NEWER
            return rb.angularDamping;
#else
            return rb.angularDrag;
#endif
        }
        
        internal static float SetAngularDamping(this Rigidbody2D rb, float damping) {
#if UNITY_6000_0_OR_NEWER
            return rb.angularDamping = damping;
#else
            return rb.angularDrag = damping;
#endif
        }

        public static Vector2 GetLocalLinearVelocity(this Rigidbody2D rb) {
            Vector2 worldVelocity = rb.GetLinearVelocity();
            Quaternion correction = Quaternion.Euler(0, 0, -rb.rotation);
            return correction * worldVelocity;
        }

        public static void SetLocalLinearVelocity(this Rigidbody2D rb, Vector2 localLinearVelocity) {
            rb.SetLinearVelocity(rb.GetRotationAsQuaternion() * localLinearVelocity);
        }

        public static Quaternion GetRotationAsQuaternion(this Rigidbody2D rb) {
            return Quaternion.Euler(0, 0, rb.rotation);
        }
    }
}