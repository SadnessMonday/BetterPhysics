using System;
using System.Collections.Generic;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.PID {
    /**
     * Warning, this implementation assumes Time.timeScale and Time.fixedDeltaTime do not change.
     */
    [RequireComponent(typeof(BetterRigidbody))]
    public class PIDFollower : MonoBehaviour {
        private BetterRigidbody brb;

        private bool targetHasRb = false;

        [SerializeField] private Transform target;

        [SerializeField] [HideInInspector] private Rigidbody targetRb;

        [Tooltip("The maximum amount of force the follower is allowed to use to accelerate")] 
        [SerializeField] private float maxAccelerationForce; // the maximum amount of force

        [SerializeField] private float maxSpeed = 10f;

        [SerializeField] private Vector3 targetOffset;
        [SerializeField] private bool offsetInLocalSpace = false;

        // A RingBuffer or something would be better
        private List<Vector3> targetPositionHistory; // Used to calculate velocity and potentially acceleration
        private Vector3 cachedTargetVelocity = Vector3.zero;
        [SerializeField] private int positionHistorySampleCount = 2;
        [SerializeField] private float rotationSpeed = 100;

        public Vector3 TargetPosition => offsetInLocalSpace
            ? target.TransformPoint(targetOffset)
            : target.position + targetOffset;

        public Transform Target {
            get => target;
            set {
                
                target = value;
                // Use of "is" intentional here.
                if (value == null) return;
                
                targetHasRb = target.TryGetComponent(out targetRb);
                if (!targetHasRb) {
                    targetPositionHistory ??= new();
                    // reset target tracking
                    targetPositionHistory.Clear();
                    cachedTargetVelocity = Vector3.zero;
                }
            }
        }

        Vector3 GetCurrentTargetPosition() {
            if (targetRb != null) return targetRb.position;

            return target.position;
        }

        Vector3 GetTargetVelocity() {
            if (targetRb != null) return targetRb.velocity;

            return CalculateAverageVelocityFromPositionHistory();
        }

        void RecordPositionHistory() {
            if (targetRb != null) return;

            targetPositionHistory.Add(GetCurrentTargetPosition());
            int countToRemove = targetPositionHistory.Count - positionHistorySampleCount;
            if (countToRemove > 0) {
                targetPositionHistory.RemoveRange(0, countToRemove);
            }
        }

        Vector3 CalculateAverageVelocityFromPositionHistory() {
            Vector3 totalDelta = default;
            int sampleCount = 0;

            bool skipFirst = true;
            Vector3 previousPos = default;
            // calculate from position history
            foreach (Vector3 pos in targetPositionHistory) {
                if (skipFirst) {
                    skipFirst = false;
                    previousPos = pos;
                    continue;
                }

                Vector3 deltaPosition = pos - previousPos;
                totalDelta += deltaPosition;
                sampleCount++;

                previousPos = pos;
            }

            Vector3 perSample = sampleCount == 0 ? Vector3.zero : totalDelta / sampleCount;
            return perSample / Time.fixedDeltaTime;
        }

        private void OnValidate() {
            Target = target;
        }

        private void Awake() {
            brb = GetComponent<BetterRigidbody>();

            Target = target;
        }

        private Vector3 intercept;
        private void FixedUpdate() {
            Vector3 positionalError = target.position - brb.position;

            Vector3 targetVelocity = GetTargetVelocity();
            Vector3 myVelocity = brb.velocity;

            Vector3 interceptVelocity = CalculateIntercept(GetCurrentTargetPosition(),
                GetTargetVelocity(),
                brb.position,
                myVelocity.magnitude);
            
            brb.angularVelocity = Vector3.zero;
            Quaternion targetRotation = Quaternion.LookRotation(interceptVelocity, brb.Up);
            Quaternion newRotation = Quaternion.RotateTowards(
                brb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
            brb.MoveRotation(newRotation);
            
            Debug.Log($"Intercept velocity: {interceptVelocity} position diff: {positionalError} forward is: {brb.Forward}");
            // accelerate iff the intercept velocity is in our general direction
            if (Vector3.Dot(interceptVelocity, brb.Forward) > 0) {
                // print("Adding force");
                brb.AddForce(brb.Forward * maxAccelerationForce);
            }

            Debug.DrawRay(brb.position, interceptVelocity, Color.green, Time.fixedDeltaTime);
        }

        private Vector3 CalculateDesiredVelocity() {
            throw new NotImplementedException();
        }

        private Quaternion CalculateDesiredRotation() {
            throw new NotImplementedException();
        }

        Vector3 CalculateIntercept(Vector3 targetLocation, Vector3 targetVelocity, Vector3 interceptorLocation,
            float interceptorSpeed) {
            float Ax = targetLocation.x;
            float Ay = targetLocation.y;
            float Az = targetLocation.z;

            float As = targetVelocity.magnitude;
            float targetSqrMag = targetVelocity.sqrMagnitude;
            Vector3 Av = Vector3.Normalize(targetVelocity);
            float Avx = Av.x;
            float Avy = Av.y;
            float Avz = Av.z;
            
            float Bx = interceptorLocation.x;
            float By = interceptorLocation.y;
            float Bz = interceptorLocation.z;

            float Bs = interceptorSpeed;
            
            float a = targetSqrMag * Avx * Avx +
                      targetSqrMag * Avy * Avy +
                      targetSqrMag * Avz * Avz -
                      interceptorSpeed * interceptorSpeed;

            if (a == 0) {
                // Debug.Log("Quadratic formula not applicable");
                return targetLocation;
            }

            float b = As * Avx * Ax +
                      As * Avy * Ay +
                      As * Avz * Az +
                      As * Avx * Bx +
                      As * Avy * By +
                      As * Avz * Bz;

            float c = Ax * Ax +
                      Ay * Ay +
                      Az * Az -
                      Ax * Bx -
                      Ay * By -
                      Az * Bz +
                      Bx * Bx +
                      By * By +
                      Bz * Bz;

            float quadraticFactor = Mathf.Sqrt(b * b - 4 * a * c);
            var twoA = 2 * a;
            float t1 = (-b + quadraticFactor) / twoA;
            float t2 = (-b - quadraticFactor) / twoA;

            // Debug.Log("t1 = " + t1 + "; t2 = " + t2);


            float t;
            if (t1 <= 0 || float.IsInfinity(t1) || float.IsNaN(t1))
                if (t2 <= 0 || float.IsInfinity(t2) || float.IsNaN(t2))
                    return targetLocation;
                else
                    t = t2;
            else if (t2 <= 0 || float.IsInfinity(t2) || float.IsNaN(t2) || t2 > t1)
                t = t1;
            else
                t = t2;

            // Debug.Log("t = " + t);
            // Debug.Log("Bs = " + Bs);

            var tbsSqr = t * (Bs * Bs);
            var tTimesAs = t * As;
            float bvx = (Ax - Bx + (tTimesAs + Avx)) / tbsSqr;
            float bvy = (Ay - By + (tTimesAs + Avy)) / tbsSqr;
            float bvz = (Az - Bz + (tTimesAs + Avz)) / tbsSqr;

            Vector3 bv = new Vector3(bvx, bvy, bvz);

            // Debug.Log("||Bv|| = (Should be 1) " + bv.magnitude);

            return bv * Bs;
        }
    }
}