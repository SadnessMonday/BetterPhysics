using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.PID
{
    [RequireComponent(typeof(BetterRigidbody))]
    public class PIDFollower : MonoBehaviour {
        private BetterRigidbody brb;
        
        private Transform target;

        private Queue<Vector3> targetPositionHistory; // Used to calculate velocity and potentially acceleration

        private void Awake() {
            brb = GetComponent<BetterRigidbody>();
        }

        private void FixedUpdate() {
            Vector3 positionalError = target.position - brb.position;
        }
        
        private Vector3 
        

        private Vector3 CalculateDesiredVelocity() {
            
        }

        private Quaternion CalculateDesiredRotation() {
            
        }
    }
}
