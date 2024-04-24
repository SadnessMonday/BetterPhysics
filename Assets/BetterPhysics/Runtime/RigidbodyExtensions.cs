using UnityEngine;
using static SadnessMonday.BetterPhysics.Utilities.ForceUtilities;

namespace SadnessMonday.BetterPhysics {
    public static class RigidbodyExtensions {
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
            rb.velocity += velocityChange;

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
            rb.velocity += velocityChange;

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