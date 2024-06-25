using UnityEngine;

namespace SadnessMonday.BetterPhysics.Utilities {
    public static class ForceUtilities {
        
        public static Vector3 ConvertVelocityChangeToNewtons(in Vector3 velocityChange, float mass) {
            return ConvertVelocityChangeToNewtons(velocityChange, mass, Time.fixedDeltaTime);
        }
        
        public static Vector3 ConvertVelocityChangeToNewtons(in Vector3 velocityChange, float mass, float deltaTime) {
            return velocityChange / deltaTime * mass;
        }
        
        public static Vector3 CalculateVelocityChange(in Vector3 force, float mass, ForceMode mode) {
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

        public static Vector3 CalculateNewtons(in Vector3 force, float mass, ForceMode mode) {
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
        
        public static Vector2 CalculateVelocityChange(in Vector2 force, float mass, ForceMode2D mode) {
            switch (mode) {
                case ForceMode2D.Force:
                    return (force * Time.fixedDeltaTime) / mass;
                case ForceMode2D.Impulse:
                    return force / mass;
                default:
                    throw new System.Exception($"Unknown force mode " + mode);
            }
        }

        public static Vector2 CalculateNewtons(in Vector2 force, float mass, ForceMode2D mode) {
            switch (mode) {
                case ForceMode2D.Force:
                    return force;
                case ForceMode2D.Impulse:
                    return force * mass;
                default:
                    throw new System.Exception($"Unknown force mode " + mode);
            }
        }

        public static Vector2 CalculateNewtons(in Vector2 oldVelocity, in Vector2 newVelocity, float mass, float deltaTime) {
            Vector2 diff = newVelocity - oldVelocity;
            return diff / deltaTime * mass;
        }
    }
}