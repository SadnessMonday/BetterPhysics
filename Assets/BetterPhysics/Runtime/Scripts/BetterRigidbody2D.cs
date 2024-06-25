using System;
using System.Collections;
using System.Collections.Generic;
using SadnessMonday.BetterPhysics.Utilities;
using UnityEngine;

using static SadnessMonday.BetterPhysics.Utilities.ForceUtilities;
using static SadnessMonday.BetterPhysics.Utilities.MathUtilities;

namespace SadnessMonday.BetterPhysics {
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(10002)]
    public class BetterRigidbody2D : MonoBehaviour {
        private static readonly List<SpeedLimit> DeferredHardLimits = new();

        private static readonly WaitForFixedUpdate YieldIns = new();

        private Rigidbody2D _rb;
        private Coroutine velocityMeasurementCoroutine = null;
        private Vector2 velocityBeforeSim;
        private Vector2 velocityAfterSim;
        private Vector2 accumulatedForce;
        
        // Exempt from local limits
        private Vector2 _exemptedVelocityChange;

        [SerializeField] List<SpeedLimit> limits = new();

        public IReadOnlyList<SpeedLimit> Limits => limits;

        public void SetLimits(IEnumerable<SpeedLimit> limits) {
            this.limits.Clear();
            AddLimits(limits);
        }

        public void SetLimits(params SpeedLimit[] limits) {
            SetLimits((IEnumerable<SpeedLimit>)limits);
        }

        public void AddLimits(IEnumerable<SpeedLimit> limits) {
            this.limits.AddRange(limits);
        }

        public void AddLimits(params SpeedLimit[] limits) {
            AddLimits((IEnumerable<SpeedLimit>)limits);
        }

        public void ClearLimits() {
            limits.Clear();
        }

        public SpeedLimit GetLimit(int index) {
            return limits[index];
        }

        public void SetLimit(int index, SpeedLimit limit) {
            limits[index] = limit;
        }

        public int LimitCount => limits.Count;

        // Start is called before the first frame update
        private void Awake() {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable() {
            velocityMeasurementCoroutine = StartCoroutine(MeasureVelocity());
        }

        private void OnDisable() {
            StopCoroutine(velocityMeasurementCoroutine);
        }


        private void Start() {
            Debug.Log($"Velocity before: {_rb.velocity}");
            _rb.AddForce(Vector2.right * _rb.mass, ForceMode2D.Impulse);
            Debug.Log($"Velocity after: {_rb.velocity}");
        }

        private void FixedUpdate() {
            accumulatedForce = CalculateNewtons(_rb.velocity, velocityAfterSim, _rb.mass);

            if (!_rb.isKinematic) {
                // Apply exempted newtons directly to the velocity
                _rb.AddLinearVelocity(_exemptedVelocityChange);
            }

            _exemptedVelocityChange = Vector2.zero;

            ApplyLimits();

            velocityBeforeSim = _rb.velocity;
        }
        
        public void ApplyLimits() {
            DeferredHardLimits.Clear();
            
            foreach (SpeedLimit limit in limits) {
                switch (limit.LimitType) {
                    case LimitType.Hard:
                        // Defer hard limits
                        DeferredHardLimits.Add(limit);
                        break;
                    case LimitType.Soft:
                        // Apply soft limits immediately
                        ApplyLimit(limit);
                        break;
                }
            }

            // Apply the deferred hard limits.
            foreach (SpeedLimit limit in DeferredHardLimits) {
                ApplyLimit(limit);
            }
        }
        
        
        void ApplyLimit(SpeedLimit limit) {
            switch (limit.LimitType) {
                case LimitType.Hard:
                    ApplyHardLimit(limit);
                    break;
                case LimitType.Soft:
                    ApplySoftLimit(limit);
                    break;
            }
        }

        private void ApplySoftLimit(SpeedLimit limit) {
            // Accumulated force is always expressed in terms of newtons. As if applied with ForceMode.Force.
            var accumulatedNewtons = _rb.GetAccumulatedForce();
            var currentVelocity = _rb.GetLinearVelocity();
            var expectedVelocityChange = CalculateVelocityChange(accumulatedNewtons, _rb.mass,
                ForceMode.Force);

            switch (limit.Directionality) {
                case Directionality.Omnidirectional: {
                    Vector2 limitedChange = CalculateVelocityChangeWithSoftLimit(currentVelocity, accumulatedNewtons,
                        limit.ScalarLimit);
                    Vector2 diff = limitedChange - expectedVelocityChange;
                    _rb.AddForce(diff, ForceMode.VelocityChange);
                    break;
                }
                // return AddForceWithOmnidirectionalLimit(force, limits.ScalarLimit, mode);
                case Directionality.WorldAxes: {
                    Vector2 min;
                    Vector2 max = limit.Max;
                    if (limit.Asymmetrical) {
                        min = limit.Min;
                    }
                    else {
                        min = -limit.Max;
                    }

                    // This is what we want the new velocity to be.
                    Vector2 newVelocity = SoftClamp(currentVelocity, expectedVelocityChange, limit.AxisLimited,
                        min, max);
                    // This is what we want the velocity diff to be
                    Vector2 clampedVelocityDiff = newVelocity - currentVelocity;
                    Vector2 correction = clampedVelocityDiff - expectedVelocityChange;
                    _rb.AddForce(correction, ForceMode.VelocityChange);
                    break;
                }
                case Directionality.LocalAxes: {
                    Vector2 min;
                    Vector2 max = limit.Max;
                    if (limit.Asymmetrical) {
                        min = limit.Min;
                    }
                    else {
                        min = -limit.Max;
                    }

                    // Convert everything to local
                    Quaternion inverseRot = Quaternion.Inverse(_rb.rotation);
                    Vector3 localForce = inverseRot * accumulatedNewtons;
                    Vector3 localVelocity = inverseRot * currentVelocity;

                    // Do the math in local space
                    Vector3 expectedLocalChange = CalculateVelocityChange(localForce, _rb.mass, ForceMode.Force);
                    Vector3 newLocalVelocity = SoftClamp(localVelocity, expectedLocalChange, limit.AxisLimited, min, max);
                    // print($"Clamped new local velocity is {newLocalVelocity}");

                    // Convert back to worldspace
                    Vector3 newWorldVelocity = _rb.rotation * newLocalVelocity;
                    Vector3 worldChange = newWorldVelocity - currentVelocity;
                    Vector3 correction = worldChange - expectedVelocityChange;
                    _rb.AddForce(correction, ForceMode.VelocityChange);
                    break;
                }
                default:
                    throw new NotImplementedException($"Unknown limit type: {limit.Directionality.ToString()}");
            }
        }

        private void ApplyHardLimit(SpeedLimit limit) {
            var currentVelocity = _rb.GetLinearVelocity();
            var accumulatedNewtons = _rb.GetAccumulatedForce();
            var expectedVelocityChange = CalculateVelocityChange(accumulatedNewtons, _rb.mass,
                ForceMode.Force);
            var expectedNewVelocity = currentVelocity + expectedVelocityChange;
            
            switch (limit.Directionality) {
                case Directionality.Omnidirectional:
                    Vector3 clampedVelocity = Vector3.ClampMagnitude(expectedNewVelocity, limit.ScalarLimit);
                    
                    // We're hard clamping so remove all accumulated force
                    _rb.AddForce(-accumulatedNewtons);
                    _rb.SetLinearVelocity(clampedVelocity);
                    break;
                case Directionality.WorldAxes: {
                    Vector3 clampedNewVelocity = expectedNewVelocity;
                    var max = limit.Max;
                    var min = limit.Min;
                    for (int i = 0; i < 3; i++) {
                        if (limit.Asymmetrical) {
                            clampedNewVelocity[i] = Mathf.Clamp(clampedNewVelocity[i], min[i], max[i]);
                        }
                        else {
                            clampedNewVelocity[i] = Mathf.Clamp(clampedNewVelocity[i], -max[i], max[i]);
                        }
                    }
                    
                    // We're hard clamping so remove all accumulated force
                    _rb.AddForce(-accumulatedNewtons);
                    // We're going to do this clamping no matter what.
                    _rb.SetLinearVelocity(clampedNewVelocity);
                    
                    break;
                }
                case Directionality.LocalAxes: {
                    Vector3 clampedLocalVelocity = Quaternion.Inverse(_rb.rotation) * expectedNewVelocity;
                    var max = limit.Max;
                    var min = limit.Min;
                    for (int i = 0; i < 3; i++) {
                        if (limit.Asymmetrical) {
                            clampedLocalVelocity[i] = Mathf.Clamp(clampedLocalVelocity[i], min[i], max[i]);
                        }
                        else {
                            clampedLocalVelocity[i] = Mathf.Clamp(clampedLocalVelocity[i], -max[i], max[i]);
                        }
                    }

                    Vector3 clampedWorldVelocity = _rb.rotation * clampedLocalVelocity;

                    // We're hard clamping so remove all accumulated force
                    _rb.AddForce(-accumulatedNewtons);
                    _rb.SetLinearVelocity(clampedWorldVelocity);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        IEnumerator MeasureVelocity() {
            while (true) {
                yield return YieldIns;
                velocityAfterSim = _rb.velocity;
            }
        }

        public void AddForceWithoutLimit(Vector2 force, ForceMode2D mode = ForceMode2D.Force) {
            Vector2 velocityChange = CalculateVelocityChange(force, _rb.mass, mode);
            _exemptedVelocityChange += velocityChange;
        }

        public void AddRelativeForceWithoutLimit(Vector2 force, ForceMode2D mode = ForceMode2D.Force) {
            force = _rb.rotation * force;
            AddForceWithoutLimit(force, mode);
        }
        
        static Vector2 SoftClamp(in Vector2 currentVelocity, in Vector2 expectedChange, in Bool3 limited, in Vector2 min, in Vector2 max) {
            Vector2 newVelocity = currentVelocity;
            for (int i = 0; i < 2; i++) {
                bool axisLimited = limited[i];
                // for each axis...
                float currentAxisVelocity = currentVelocity[i];
                float expectedAxisChange = expectedChange[i];
                float expectedNewVelocity = expectedAxisChange + currentAxisVelocity;
                
                // Less than 0 is no limit
                // Or If expected change is 0, we're just going to have no change
                if (!axisLimited || expectedAxisChange == 0) {
                    newVelocity[i] = expectedNewVelocity;
                    continue;
                }
                
                float axisMin = min[i];
                float axisMax = max[i];
                
                float clampA, clampB;
                if (currentAxisVelocity < axisMin) {
                    clampA = currentAxisVelocity;
                    clampB = axisMax;
                }
                else if (currentAxisVelocity > axisMax) {
                    clampA = currentAxisVelocity;
                    clampB = axisMin;
                }
                else {
                    clampA = axisMin;
                    clampB = axisMax;
                }

                newVelocity[i] = MathUtilities.DirectionalClamp(expectedNewVelocity, clampA, clampB);
            }

            // Debug.Log($"curr: {currentVelocity}, expectedChange: {expectedChange}, limits: {limits}, new: {newVelocity}");
            return newVelocity;
        }
    }
}
