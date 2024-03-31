using UnityEngine;

namespace SadnessMonday.BetterPhysics.Utilities {
    public static class ForceUtilities {
        
        public static Vector3 ConvertVelocityChangeToNewtons(Vector3 velocityChange, float mass) {
            return ConvertVelocityChangeToNewtons(velocityChange, mass, Time.fixedDeltaTime);
        }
        
        public static Vector3 ConvertVelocityChangeToNewtons(Vector3 velocityChange, float mass, float deltaTime) {
            return velocityChange / deltaTime * mass;
        }
        
        public static Vector3 CalculateVelocityChange(Vector3 force, float mass, ForceMode mode) {
            switch (mode) {
                case ForceMode.Force:
                    return (force * Time.fixedDeltaTime) / mass;
                case ForceMode.Acceleration:
                    return force * Time.fixedDeltaTime;
                case ForceMode.Impulse:
                    return force / mass;
                case ForceMode.VelocityChange:
                    return force;
                default:
                    throw new System.Exception($"Unknown force mode " + mode);
            }
        }

        public static Vector3 CalculateNewtons(Vector3 force, float mass, ForceMode mode) {
            switch (mode) {
                case ForceMode.Force:
                    return force;
                case ForceMode.Acceleration:
                    return force / Time.fixedDeltaTime;
                case ForceMode.Impulse:
                    return force * mass;
                case ForceMode.VelocityChange:
                    return force / Time.fixedDeltaTime * mass;
                default:
                    throw new System.Exception($"Unknown force mode " + mode);
            }
        }
    }
}