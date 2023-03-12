using System;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Samples {
    
    [RequireComponent(typeof(BetterRigidbody))]
    public class BasicHomingMissile : MonoBehaviour {
        private BetterRigidbody brb;
        public Transform target;
        [SerializeField] private float desiredSpeed = 15f;
        [SerializeField] private float acceleration = 5f;
        [SerializeField] private float angularAcceleration = 10f;

        private void Awake() {
            brb = GetComponent<BetterRigidbody>();
        }

        private void Start() {
            brb.hardLimits = Limits.Default;
            brb.softLimits = Limits.OmnidirectionalLimit(desiredSpeed);
        }

        private void FixedUpdate() {
            Vector3 direction = (target.position - brb.position).normalized;
            
            AdjustVelocity(direction);
            AdjustOrientation(direction);
        }

        private void AdjustOrientation(Vector3 direction) {
            Vector3 currentOrientation = brb.Forward;
            Vector3 currentAngularVelocity = brb.angularVelocity;

            Vector3 desiredOrientation = direction;
            Vector3 orientationDifference = desiredOrientation - currentOrientation;

            float angleDiff = Vector3.Angle(currentOrientation, desiredOrientation);

            // when we're close to the desired orientation we want a small angular velocity.
            // when we're far, we want a large velocity
            Vector3 desiredAngularVelocity = orientationDifference.normalized * angleDiff;
            Vector3 angularVelocityDiff = desiredAngularVelocity - currentAngularVelocity;

            brb.AddTorque(angularVelocityDiff.normalized * angularAcceleration, ForceMode.Acceleration);
        }

        void AdjustVelocity(Vector3 direction) {
            Vector3 desiredVelocity = direction * desiredSpeed;
            Vector3 currentVelocity = brb.velocity;

            Vector3 diff = (desiredVelocity - currentVelocity).normalized;
            brb.AddForce(diff * acceleration, ForceMode.Acceleration);
        }
    }
}