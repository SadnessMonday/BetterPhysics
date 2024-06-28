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
        
        public Vector2 Velocity {
            get => _rb.GetLinearVelocity();
            set => _rb.SetLinearVelocity(value);
        }

        public Vector2 LocalVelocity {
            get => _rb.GetInverseRotationAsQuaternion() * _rb.GetLinearVelocity();
            set => _rb.SetLinearVelocity(_rb.GetRotationAsQuaternion() * value);
        }

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

        private void FixedUpdate() {
            // Debug.Log($"{GetInstanceID()} Frame {Time.fixedTime} Velocity after the simulation {velocityAfterSim} Velocity now: {_rb.velocity}");
            accumulatedForce = CalculateNewtons(velocityAfterSim, _rb.velocity, _rb.mass, Time.fixedDeltaTime);

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
                        ApplySoftLimit(limit);
                        break;
                }
            }

            // Apply the deferred hard limits.
            foreach (SpeedLimit limit in DeferredHardLimits) {
                ApplyHardLimit(limit);
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
            var accumulatedNewtons = accumulatedForce;
            var currentVelocity = velocityAfterSim;
            var expectedVelocityChange = CalculateVelocityChange(accumulatedNewtons, _rb.mass,
                ForceMode2D.Force);

            switch (limit.Directionality) {
                case Directionality.Omnidirectional: {
                    Vector2 limitedChange = CalculateVelocityChangeWithSoftLimit(currentVelocity, accumulatedNewtons,
                        limit.ScalarLimit);
                    Vector2 diff = limitedChange - expectedVelocityChange;
                    _rb.AddLinearVelocity(diff);
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
                    _rb.velocity += correction;
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
                    Quaternion inverseRot = _rb.GetInverseRotationAsQuaternion();
                    Vector2 localForce = inverseRot * accumulatedNewtons;
                    Vector2 localVelocity = inverseRot * currentVelocity;

                    // Do the math in local space
                    Vector2 expectedLocalChange = CalculateVelocityChange(localForce, _rb.mass, ForceMode2D.Force);
                    Vector2 newLocalVelocity = SoftClamp(localVelocity, expectedLocalChange, limit.AxisLimited, min, max);
                    // print($"Clamped new local velocity is {newLocalVelocity}");

                    // Convert back to worldspace
                    Vector2 newWorldVelocity = _rb.GetRotationAsQuaternion() * newLocalVelocity;
                    Vector2 worldChange = newWorldVelocity - currentVelocity;
                    Vector2 correction = worldChange - expectedVelocityChange;
                    _rb.velocity += correction;
                    break;
                }
                default:
                    throw new NotImplementedException($"Unknown limit type: {limit.Directionality.ToString()}");
            }
        }

        private void ApplyHardLimit(SpeedLimit limit) {
            var currentVelocity = _rb.GetLinearVelocity();
            
            switch (limit.Directionality) {
                case Directionality.Omnidirectional:
                    Vector2 clampedVelocity = Vector2.ClampMagnitude(currentVelocity, limit.ScalarLimit);
                    
                    _rb.SetLinearVelocity(clampedVelocity);
                    break;
                case Directionality.WorldAxes: {
                    Vector2 clampedNewVelocity = currentVelocity;
                    Vector2 max = limit.Max;
                    Vector2 min = limit.Min;
                    for (int i = 0; i < 2; i++) {
                        if (limit.Asymmetrical) {
                            clampedNewVelocity[i] = Mathf.Clamp(clampedNewVelocity[i], min[i], max[i]);
                        }
                        else {
                            clampedNewVelocity[i] = Mathf.Clamp(clampedNewVelocity[i], -max[i], max[i]);
                        }
                    }
                    
                    // We're going to do this clamping no matter what.
                    _rb.SetLinearVelocity(clampedNewVelocity);
                    
                    break;
                }
                case Directionality.LocalAxes: {
                    Vector2 clampedLocalVelocity = _rb.GetLocalLinearVelocity();
                    Vector2 max = limit.Max;
                    Vector2 min = limit.Min;
                    for (int i = 0; i < 2; i++) {
                        if (limit.Asymmetrical) {
                            clampedLocalVelocity[i] = Mathf.Clamp(clampedLocalVelocity[i], min[i], max[i]);
                        }
                        else {
                            clampedLocalVelocity[i] = Mathf.Clamp(clampedLocalVelocity[i], -max[i], max[i]);
                        }
                    }
                    
                    _rb.SetLocalLinearVelocity(clampedLocalVelocity);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        IEnumerator MeasureVelocity() {
            while (true) {
                yield return YieldIns;
                // Debug.Log($"Frame {Time.fixedTime} Velocity measured after the simulation {velocityAfterSim}");
                velocityAfterSim = _rb.velocity;
            }
        }

        public void AddForceWithoutLimit(Vector2 force, ForceMode2D mode = ForceMode2D.Force) {
            Vector2 velocityChange = CalculateVelocityChange(force, _rb.mass, mode);
            _exemptedVelocityChange += velocityChange;
        }

        public void AddRelativeForceWithoutLimit(Vector2 force, ForceMode2D mode = ForceMode2D.Force) {
            force = _rb.GetRotationAsQuaternion() * force;
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

                newVelocity[i] = DirectionalClamp(expectedNewVelocity, clampA, clampB);
            }

            // Debug.Log($"curr: {currentVelocity}, expectedChange: {expectedChange}, limits: {limits}, new: {newVelocity}");
            return newVelocity;
        }
        
        private Vector2 CalculateVelocityChangeWithSoftLimit(in Vector2 currentVelocity, in Vector2 force,
            float speedLimit, ForceMode2D mode = ForceMode2D.Force) {
            // velocityChange is how much we would be adding to the velocity if we were to just add it normally.
            Vector2 velocityChange = CalculateVelocityChange(force, _rb.mass, mode);
            if (speedLimit < 0 || float.IsPositiveInfinity(speedLimit)) {
                // Speed limit < 0 counts as no limit.
                return velocityChange;
            }

            // expected result is what we would end up with if we just added that directly.
            Vector2 expectedResult = currentVelocity + velocityChange;
            Vector2 excess = expectedResult - Vector2.ClampMagnitude(expectedResult, speedLimit);
            float excessSpeed = excess.magnitude;

            Vector2 component = Vector3.Project(velocityChange, excess);


            float reductionAmount = Mathf.Min(excessSpeed, component.magnitude);
            Vector2 adjustedComponent = component - (reductionAmount * expectedResult.normalized);

            // take out the component in the direction of the overspeed and add the adjusted one.
            Vector2 effectiveVelocityChange = velocityChange - component + adjustedComponent;
            return effectiveVelocityChange;
        }
        
        #region Rigidbody2D property pass-through

        public float angularDrag {
            get => _rb.GetAngularDamping();
            set => _rb.SetAngularDamping(value);
        }

        public float angularVelocity {
            get => _rb.angularVelocity;
            set => _rb.angularVelocity = value;
        }

        public Vector2 centerOfMass {
            get => _rb.centerOfMass;
            set => _rb.centerOfMass = value;
        }

        public CollisionDetectionMode2D collisionDetectionMode {
            get => _rb.collisionDetectionMode;
            set => _rb.collisionDetectionMode = value;
        }

        public RigidbodyConstraints2D constraints {
            get => _rb.constraints;
            set => _rb.constraints = value;
        }

        public float drag {
            get => _rb.GetLinearDamping();
            set => _rb.SetLinearDamping(value);
        } // TODO we should probably not use normal RB drag as it won't play well with our stuff?

        public bool freezeRotation {
            get => _rb.freezeRotation;
            set => _rb.freezeRotation = value;
        }

        public float inertia {
            get => _rb.inertia;
            set => _rb.inertia = value;
        }

        public RigidbodyInterpolation2D interpolation {
            get => _rb.interpolation;
            set => _rb.interpolation = value;
        }

        public bool isKinematic {
            get => _rb.isKinematic;
            set => _rb.isKinematic = value;
        }

        public float mass {
            get => _rb.mass;
            set => _rb.mass = value;
        }

        public Vector2 position {
            get => _rb.position;
            set => _rb.position = value;
        }

        public float rotation {
            get => _rb.rotation;
            set => _rb.rotation = value;
        }

        public RigidbodySleepMode2D sleepMode {
            get => _rb.sleepMode;
            set => _rb.sleepMode = value;
        }

        public float gravityScale {
            get => _rb.gravityScale;
            set => _rb.gravityScale = value;
        }

        public Vector2 velocity {
            get => _rb.GetLinearVelocity();
            set => _rb.SetLinearVelocity(value);
        }

        public Vector2 worldCenterOfMass => _rb.worldCenterOfMass;

        #endregion

        #region Method pass-through
        
        /// <summary>
        /// Pass-through of Rigidbody.AddForce
        /// </summary>
        /// <param name="force"></param>
        /// <param name="mode"></param>
        public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force) {
            _rb.AddForce(force, mode);
        }

        /// <summary>
        /// Pass-through of Rigidbody.AddRelativeForce
        /// </summary>
        /// <param name="force"></param>
        /// <param name="mode"></param>
        public void AddRelativeForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force) {
            _rb.AddRelativeForce(force, mode);
        }
        
        /// <summary>
        /// Pass-through of Rigidbody.AddTorque
        /// </summary>
        /// <param name="force"></param>
        /// <param name="mode"></param>
        public void AddTorque(float torque, ForceMode2D mode = ForceMode2D.Force) {
            _rb.AddTorque(torque, mode);
        }

        public Func<Vector2, Vector2> ClosestPoint => _rb.ClosestPoint;
        public Func<Vector2, Vector2> GetPointVelocity => _rb.GetPointVelocity;
        public Func<Vector2, Vector2> GetRelativePointVelocity => _rb.GetRelativePointVelocity;
        public Func<bool> IsSleeping => _rb.IsSleeping;
        public Action<Vector2> MovePosition => _rb.MovePosition;
        public Action<Quaternion> MoveRotation => _rb.MoveRotation;
        public Action Sleep => _rb.Sleep;
        
        // TODO all the RB.Cast functions

        public Action WakeUp => _rb.WakeUp;

        #endregion
    }
}
