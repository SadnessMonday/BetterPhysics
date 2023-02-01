using System;
using System.Collections.Generic;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.PID {
    [RequireComponent(typeof(BetterRigidbody))]
    public class PIDFollower : MonoBehaviour {
        private BetterRigidbody brb;

        private bool targetHasRb = false;
        
        [SerializeField]
        private Transform target;
        [SerializeField]
        [HideInInspector]
        private Rigidbody targetRb;

        private Queue<Vector3> targetPositionHistory; // Used to calculate velocity and potentially acceleration

        public Transform Target {
            get => target;
            set {
                target = value;
                targetHasRb = target.TryGetComponent(out targetRb);
            }
        }
        
        Vector3 GetCurrentTargetPosition() {
            if (targetRb != null) return targetRb.position;

            return target.position;
        }

        private void OnValidate() {
            Target = target;
        }

        private void Awake() {
            brb = GetComponent<BetterRigidbody>();
            
            Target = target;
        }

        private void FixedUpdate() {
            Vector3 positionalError = target.position - brb.position;
        }

        private Vector3 CalculateDesiredVelocity() {
            throw new NotImplementedException();
        }

        private Quaternion CalculateDesiredRotation() {
            throw new NotImplementedException();
        }
    }
}