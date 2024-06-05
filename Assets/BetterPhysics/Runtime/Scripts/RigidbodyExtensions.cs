using UnityEngine;
using static SadnessMonday.BetterPhysics.Utilities.ForceUtilities;
using Vector3 = UnityEngine.Vector3;

namespace SadnessMonday.BetterPhysics {
    public static class RigidbodyExtensions {

        internal static Vector3 AddLinearVelocity(this Rigidbody rb, in Vector3 addend) {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity += addend;
#else
            return rb.velocity += addend;
#endif
        }
        
        internal static Vector3 GetLinearVelocity(this Rigidbody rb) {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }
        
        internal static Vector3 SetLinearVelocity(this Rigidbody rb, Vector3 velocity) {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity = velocity;
#else
            return rb.velocity = velocity;
#endif
        }
        
        internal static float GetLinearDamping(this Rigidbody rb) {
#if UNITY_6000_0_OR_NEWER
            return rb.linearDamping;
#else
            return rb.drag;
#endif
        }
        
        internal static float SetLinearDamping(this Rigidbody rb, float damping) {
#if UNITY_6000_0_OR_NEWER
            return rb.linearDamping = damping;
#else
            return rb.drag = damping;
#endif
        }
        
        internal static float GetAngularDamping(this Rigidbody rb) {
#if UNITY_6000_0_OR_NEWER
            return rb.angularDamping;
#else
            return rb.angularDrag;
#endif
        }
        
        internal static float SetAngularDamping(this Rigidbody rb, float damping) {
#if UNITY_6000_0_OR_NEWER
            return rb.angularDamping = damping;
#else
            return rb.angularDrag = damping;
#endif
        }
        
        public static Vector3 Forward(this Rigidbody rb) => rb.rotation * Vector3.forward;
        public static Vector3 Left(this Rigidbody rb) => rb.rotation * Vector3.left;
        public static Vector3 Right(this Rigidbody rb) => rb.rotation * Vector3.right;
        public static Vector3 Up(this Rigidbody rb) => rb.rotation * Vector3.up;
        public static Vector3 Down(this Rigidbody rb) => rb.rotation * Vector3.down;
        public static Vector3 Back(this Rigidbody rb) => rb.rotation * Vector3.back;

        /**
         * Adds a force and returns the effective world-space change in velocity that occurred.
         */
        public static Vector3 VisibleAddForce(this Rigidbody rb, in Vector3 force, ForceMode mode = ForceMode.Force) {
            Vector3 velocityChange = CalculateVelocityChange(force, rb.mass, mode);
            rb.AddLinearVelocity(velocityChange);

            return velocityChange;
        }

        /**
         * Adds a force and returns the effective world-space change in velocity that occurred.
         */
        public static Vector3 VisibleAddForce(this Rigidbody rb, float x, float y, float z, ForceMode mode = ForceMode.Force) {
            return rb.VisibleAddForce(new Vector3(x, y, z), mode);
        }

        /**
         * Adds a force in the body's local coordinate system and returns the effective world-space change in velocity
         * that occurred.
         */
        public static Vector3 VisibleAddRelativeForce(this Rigidbody rb, Vector3 force, ForceMode mode = ForceMode.Force) {
            force = rb.rotation * force;
            Vector3 velocityChange = CalculateVelocityChange(force, rb.mass, mode);
            rb.AddLinearVelocity(velocityChange);

            return velocityChange;
        }

        
        /**
         * Adds a force in the body's local coordinate system and returns the effective world-space change in velocity
         * that occurred.
         */
        public static Vector3 VisibleAddRelativeForce(this Rigidbody rb, float x, float y, float z, ForceMode mode = ForceMode.Force) {
            return rb.VisibleAddRelativeForce(new Vector3(x, y, z), mode);
        }
    }
}