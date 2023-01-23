using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SadnessMonday.BetterPhysics.Samples {
    
    [RequireComponent(typeof(BetterRigidbody))]
    public class DumbFollower : MonoBehaviour {
        public delegate void DeathHandler(DumbFollower source);

        public event DeathHandler OnDeath;
        
        public Transform target;
        public float forceAmount = 1f;
        
        private BetterRigidbody rb;
        [SerializeField]
        float maxSpeed = 10f;
        
        private void Awake() {
            rb = GetComponent<BetterRigidbody>();
        }

        private void Start() {
            rb.softLimitType = LimitType.Omnidirectional;
            rb.softScalarLimit = maxSpeed;
        }

        private void FixedUpdate() {
            if (target == null) return;

            Vector3 direction = (target.position - rb.position).normalized;
            rb.AddForce(forceAmount * direction);
        }

        private void OnDestroy() {
            OnDeath?.Invoke(this);
        }
    }
}