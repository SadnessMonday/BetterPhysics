using System;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Samples {
    public class BasicHomingMissile : MonoBehaviour {
        [SerializeField] private BetterRigidbody brb;
        public Transform target;
        [SerializeField] private float speed = 0;

        private void Awake() {
            brb = GetComponent<BetterRigidbody>();
        }

        private void FixedUpdate() {
            Vector3 direction = target.position - brb.position;
            Vector3 cross = Vector3.Cross(direction.normalized, brb.velocity.normalized);

            brb.angularVelocity = cross;
            brb.velocity = brb.Forward * speed;
        }
    }
}