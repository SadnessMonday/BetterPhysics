using System;
using System.Collections.Generic;
using System.Linq;
using SadnessMonday.BetterPhysics.Layers;
using Unity.Collections;
using UnityEngine;
using static SadnessMonday.BetterPhysics.Utilities.ForceUtilities;

namespace SadnessMonday.BetterPhysics {
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(10001)] // We need to run _late_ so we get a chance to modify velocity.
    public class BetterRigidbody : MonoBehaviour {
        public delegate void PhysicsLayerChangeHandler(BetterRigidbody source, int oldLayer, int newLayer);

        public event PhysicsLayerChangeHandler OnPhysicsLayerChanged;
        private static readonly List<SpeedLimit> DeferredHardLimits = new();
        
        private Rigidbody _rb;
        // Exempt from local limits
        private Vector3 _exemptedVelocityChange;
        
        [SerializeField] 
        List<SpeedLimit> limits = new();
        
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

        private void FixedUpdate() {
            if (!_rb.isKinematic) {
                // Apply exempted newtons directly to the velocity
                _rb.AddLinearVelocity(_exemptedVelocityChange);
            }
            _exemptedVelocityChange = Vector3.zero;
            
            ApplyLimits();
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
                    Vector3 limitedChange = CalculateVelocityChangeWithSoftLimit(currentVelocity, accumulatedNewtons,
                        limit.ScalarLimit);
                    Vector3 diff = limitedChange - expectedVelocityChange;
                    _rb.AddForce(diff, ForceMode.VelocityChange);
                    break;
                }
                // return AddForceWithOmnidirectionalLimit(force, limits.ScalarLimit, mode);
                case Directionality.WorldAxes: {
                    Vector3 min;
                    Vector3 max = limit.Max;
                    if (limit.Asymmetrical) {
                        min = limit.Min;
                    }
                    else {
                        min = -limit.Max;
                    }

                    // This is what we want the new velocity to be.
                    Vector3 newVelocity = SoftClamp(currentVelocity, expectedVelocityChange, limit.AxisLimited,
                        min, max);
                    // This is what we want the velocity diff to be
                    Vector3 clampedVelocityDiff = newVelocity - currentVelocity;
                    Vector3 correction = clampedVelocityDiff - expectedVelocityChange;
                    _rb.AddForce(correction, ForceMode.VelocityChange);
                    break;
                }
                case Directionality.LocalAxes: {
                    Vector3 min;
                    Vector3 max = limit.Max;
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

        internal int GetRigidbodyInstanceID() => _rb.GetInstanceID();
        internal Rigidbody WrappedRigidbody => GetComponent<Rigidbody>();

        public Vector3 Velocity {
            get => _rb.GetLinearVelocity();
            set => _rb.SetLinearVelocity(value);
        }

        public Vector3 LocalVelocity {
            get => Quaternion.Inverse(_rb.rotation) * _rb.GetLinearVelocity();
            set => _rb.SetLinearVelocity(_rb.rotation * value);
        }
        
        public float Speed => _rb.GetLinearVelocity().magnitude;

        [SerializeField] private List<OneWayLayerInteraction> layerInteractions = new();

        public bool TryGetInteraction(int receiverLayer, out OneWayLayerInteraction interaction) {
            return ContactModificationManager.Instance.TryGetCustomInteraction(this, receiverLayer, out interaction);
        }

        public Vector3 Back {
            get => _rb.rotation * Vector3.back;
            set => rotation = Quaternion.FromToRotation(Vector3.back, value);
        }

        public Vector3 Forward {
            get => _rb.rotation * Vector3.forward;
            set => rotation = Quaternion.LookRotation(value);
        }

        public Vector3 Left {
            get => _rb.rotation * Vector3.left;
            set => rotation = Quaternion.FromToRotation(Vector3.left, value);
        }

        public Vector3 Right {
            get => _rb.rotation * Vector3.right;
            set => rotation = Quaternion.FromToRotation(Vector3.right, value);
        }

        public Vector3 Down {
            get => _rb.rotation * Vector3.down;
            set => rotation = Quaternion.FromToRotation(Vector3.down, value);
        }

        public Vector3 Up {
            get => _rb.rotation * Vector3.up;
            set => rotation = Quaternion.FromToRotation(Vector3.up, value);
        }

        #region Rigidbody property pass-through

        public float angularDrag {
            get => _rb.GetAngularDamping();
            set => _rb.SetAngularDamping(value);
        }

        public Vector3 angularVelocity {
            get => _rb.angularVelocity;
            set => _rb.angularVelocity = value;
        }

        public Vector3 centerOfMass {
            get => _rb.centerOfMass;
            set => _rb.centerOfMass = value;
        }

        public CollisionDetectionMode collisionDetectionMode {
            get => _rb.collisionDetectionMode;
            set => _rb.collisionDetectionMode = value;
        }

        public RigidbodyConstraints constraints {
            get => _rb.constraints;
            set => _rb.constraints = value;
        }

        public bool detectCollisions {
            get => _rb.detectCollisions;
            set => _rb.detectCollisions = value;
        }

        public float drag {
            get => _rb.GetLinearDamping();
            set => _rb.SetLinearDamping(value);
        } // TODO we should probably not use normal RB drag as it won't play well with our stuff?

        public bool freezeRotation {
            get => _rb.freezeRotation;
            set => _rb.freezeRotation = value;
        }

        public Vector3 inertiaTensor {
            get => _rb.inertiaTensor;
            set => _rb.inertiaTensor = value;
        }

        public Quaternion inertiaTensorRotation {
            get => _rb.inertiaTensorRotation;
            set => _rb.inertiaTensorRotation = value;
        }

        public RigidbodyInterpolation interpolation {
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

        public float maxAngularVelocity {
            get => _rb.maxAngularVelocity;
            set => _rb.maxAngularVelocity = value;
        }

        public float maxDepenetrationVelocity {
            get => _rb.maxDepenetrationVelocity;
            set => _rb.maxDepenetrationVelocity = value;
        }

        public Vector3 position {
            get => _rb.position;
            set => _rb.position = value;
        }

        public Quaternion rotation {
            get => _rb.rotation;
            set => _rb.rotation = value;
        }

        public float sleepThreshold {
            get => _rb.sleepThreshold;
            set => _rb.sleepThreshold = value;
        }

        public int solverIterations {
            get => _rb.solverIterations;
            set => _rb.solverIterations = value;
        }

        public int solverVelocityIterations {
            get => _rb.solverVelocityIterations;
            set => _rb.solverVelocityIterations = value;
        }

        public bool useGravity {
            get => _rb.useGravity;
            set => _rb.useGravity = value;
        } // TODO should we hijack gravity for our own nefarious purposes?

        public Vector3 velocity {
            get => _rb.GetLinearVelocity();
            set => _rb.SetLinearVelocity(value);
        }

        public Vector3 worldCenterOfMass => _rb.worldCenterOfMass;

        #endregion

        #region Method pass-through

        /// <summary>
        /// Pass-Through of AddExplosionForce
        /// </summary>
        /// <param name="explosionForce"></param>
        /// <param name="explosionPosition"></param>
        /// <param name="explosionRadius"></param>
        /// <param name="upwardsModifier"></param>
        /// <param name="mode"></param>
        public void AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius,
            float upwardsModifier, ForceMode mode = ForceMode.Force) {
            _rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, upwardsModifier, mode);
        }
        
        /// <summary>
        /// Pass-through of Rigidbody.AddForce
        /// </summary>
        /// <param name="force"></param>
        /// <param name="mode"></param>
        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force) {
            _rb.AddForce(force, mode);
        }

        /// <summary>
        /// Pass-through of Rigidbody.AddForce
        /// </summary>
        /// <param name="force"></param>
        /// <param name="mode"></param>
        public void AddForce(float x, float y, float z, ForceMode mode = ForceMode.Force) {
            // print("Adding force");
            _rb.AddForce(x, y, z, mode);
        }

        /// <summary>
        /// Pass-through of Rigidbody.AddRelativeForce
        /// </summary>
        /// <param name="force"></param>
        /// <param name="mode"></param>
        public void AddRelativeForce(Vector3 force, ForceMode mode = ForceMode.Force) {
            _rb.AddRelativeForce(force, mode);
        }
        
        /// <summary>
        /// Pass-through of Rigidbody.AddRelativeForce
        /// </summary>
        /// <param name="force"></param>
        /// <param name="mode"></param>
        public void AddRelativeForce(float x, float y, float z, ForceMode mode = ForceMode.Force) {
            _rb.AddRelativeForce(x, y, z, mode);
        }
        
        /// <summary>
        /// Pass-through of Rigidbody.AddTorque
        /// </summary>
        /// <param name="force"></param>
        /// <param name="mode"></param>
        public void AddTorque(Vector3 torque, ForceMode mode = ForceMode.Force) {
            _rb.AddTorque(torque, mode);
        }
        
        /// <summary>
        /// Pass-through of Rigidbody.AddTorque
        /// </summary>
        /// <param name="force"></param>
        /// <param name="mode"></param>
        public void AddTorque(float x, float y, float z, ForceMode mode = ForceMode.Force) {
            _rb.AddTorque(x, y, z, mode);
        }

        /// <summary>
        /// Pass-through of Rigidbody.AddRelativeTorque
        /// </summary>
        /// <param name="force"></param>
        /// <param name="mode"></param>
        public void AddRelativeTorque(Vector3 torque, ForceMode mode = ForceMode.Force) {
            _rb.AddRelativeTorque(torque, mode);
        }

        /// <summary>
        /// Pass-through of Rigidbody.AddRelativeTorque
        /// </summary>
        /// <param name="force"></param>
        /// <param name="mode"></param>
        public void AddRelativeTorque(float x, float y, float z, ForceMode mode = ForceMode.Force) {
            _rb.AddRelativeTorque(x, y, z, mode);
        }

        public Func<Vector3, Vector3> ClosestPointOnBounds => _rb.ClosestPointOnBounds;
        public Func<Vector3, Vector3> GetPointVelocity => _rb.GetPointVelocity;
        public Func<Vector3, Vector3> GetRelativePointVelocity => _rb.GetRelativePointVelocity;
        public Func<bool> IsSleeping => _rb.IsSleeping;
        public Action<Vector3> MovePosition => _rb.MovePosition;
        public Action<Quaternion> MoveRotation => _rb.MoveRotation;
        public Action ResetCenterOfMass => _rb.ResetCenterOfMass;
        public Action ResetInertiaTensor => _rb.ResetInertiaTensor;
        public Action<float> SetDensity => _rb.SetDensity;
        public Action Sleep => _rb.Sleep;

        public bool SweepTest(Vector3 direction, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
            return _rb.SweepTest(direction, out hitInfo, maxDistance, queryTriggerInteraction);
        }

        public RaycastHit[] SweepTestAll(Vector3 direction, float maxDistance = Mathf.Infinity,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
            return _rb.SweepTestAll(direction, maxDistance, queryTriggerInteraction);
        }

        public Action WakeUp => _rb.WakeUp;

        #endregion

        #region Unity Messages

        void Awake() {
            EnsureRigidbody();
            // TODO maybe this should actually happen elsewhere
            foreach (Collider c in GetComponentsInChildren<Collider>()) {
                c.hasModifiableContacts = true;
            }
        }

        internal IDictionary<int, OneWayLayerInteraction> SerializedInteractions {
            get { return layerInteractions.ToDictionary(li => li.receiver, li => li); }
        }

        [SerializeField] private int physicsLayer = default;

        public int PhysicsLayer {
            get => physicsLayer;
            set {
                var oldValue = physicsLayer;
                physicsLayer = value;

                if (value != oldValue) {
#if UNITY_EDITOR
                    if (UnityEditor.EditorApplication.isPlaying)
#endif
                    {
                        ContactModificationManager.Instance.UpdateBodyLayer(this);
                    }
                    OnPhysicsLayerChanged?.Invoke(this, oldValue, value);
                }
            }
        }

        public void ResetCustomInteractions() {
            ContactModificationManager.Instance.ResetCustomInteractions(this);
        }

        public void SetCustomInteraction(OneWayLayerInteraction interaction) {
            ContactModificationManager.Instance.SetCustomInteraction(this, interaction);
        }

        public bool RemoveCustomInteraction(int receiverLayer) {
            return ContactModificationManager.Instance.RemoveCustomInteraction(this, receiverLayer);
        }

        private void EnsureRigidbody() {
            if (!TryGetComponent(out _rb)) {
                _rb = gameObject.AddComponent<Rigidbody>();
            }
            
            if (ReferenceEquals(null, _rb)) {
                throw new Exception(
                    "Problem creating BetterRigidbody. There may be incompatible components present, such as 2D physics components");
            }

            _rb.hideFlags = HideFlags.None;
        }

        void OnValidate() {
            EnsureRigidbody();
        }

        private void OnDestroy() {
            if (_rb != null) {
                Destroy(_rb);
            }
        }

        void OnEnable() {
            ContactModificationManager.Instance.Register(this);
            ContactModificationManager.Instance.UpdateBodyLayer(this);
        }

        void OnDisable() {
            ContactModificationManager.Instance.UnRegister(this);
        }

        void OnDrawGizmosSelected() { }

        void ModifyContacts(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs) {
            var pair = pairs[0];
            // pair.SetMaxImpulse
        }

        #endregion

        #region custom stuff

        public Vector3 AddForceWithLimit(Vector3 force, ForceMode mode, SpeedLimit limit) {
            _rb.AddForce(force, mode);
            ApplyLimit(limit);
            
            // TODO calculate the correct velocity diff here
            return Vector3.zero;
        }

        public Vector3 AddForceWithLimits(Vector3 force, ForceMode mode, params SpeedLimit[] limits) {
            return AddForceWithLimits(force, mode, (IEnumerable<SpeedLimit>)limits);
        }

        public Vector3 AddForceWithLimits(Vector3 force, ForceMode mode, IEnumerable<SpeedLimit> limits) {
            _rb.AddForce(force, mode);
            foreach (var limit in limits) ApplyLimit(limit);
            
            // TODO calculate the correct velocity diff here
            return Vector3.zero;
        }

        Vector3 AddForceWithSoftLimit(Vector3 force, SpeedLimit limits, ForceMode mode) {
            switch (limits.Directionality) {
                case Directionality.Omnidirectional:
                    return AddForceWithOmnidirectionalLimit(force, limits.ScalarLimit, mode);
                case Directionality.WorldAxes:
                    if (limits.Asymmetrical) {
                        return AddForceWithWorldAxisLimit(force, limits.AxisLimited, limits.Min, limits.Max, mode);
                    }

                    return AddForceWithWorldAxisLimit(force, limits.AxisLimited, -limits.Max, limits.Max, mode);
                case Directionality.LocalAxes:
                    if (limits.Asymmetrical) {
                        return AddForceWithLocalAxisLimit(force, limits.AxisLimited, limits.Min, limits.Max, mode);
                    }
                    
                    return AddForceWithLocalAxisLimit(force, limits.AxisLimited, -limits.Max, limits.Max, mode);
                default:
                    throw new NotImplementedException($"Unknown limit type: {limits.Directionality.ToString()}");
            }
        }
        
        private Vector3 AddRelativeForceWithSoftLimit(Vector3 force, SpeedLimit limits, ForceMode mode) {
            switch (limits.Directionality) {
                case Directionality.Omnidirectional:
                    return AddRelativeForceWithOmnidirectionalLimit(force, limits.ScalarLimit, mode);
                case Directionality.WorldAxes:
                    if (limits.Asymmetrical) {
                        return AddRelativeForceWithWorldAxisLimit(force, limits.AxisLimited, -limits.Max, limits.Max, mode);
                    }

                    return AddRelativeForceWithWorldAxisLimit(force, limits.AxisLimited, limits.Min, limits.Max, mode);
                case Directionality.LocalAxes:
                    if (limits.Asymmetrical) {
                        return AddRelativeForceWithLocalAxisLimit(force, limits.AxisLimited, -limits.Max, limits.Max, mode);
                    }

                    return AddRelativeForceWithLocalAxisLimit(force, limits.AxisLimited, limits.Min, limits.Max, mode);
                default:
                    throw new NotImplementedException($"Unknown limit type: {limits.Directionality.ToString()}");
            }
        }


        Vector3 AddRelativeForceWithLocalAxisLimit(Vector3 localForce, in Bool3 limited, in Vector3 min, in Vector3 max, ForceMode mode = ForceMode.Force) {
            // Convert everything to local
            Vector3 localVelocity = LocalVelocity;
            Vector3 worldVelocity = _rb.GetLinearVelocity();

            // Do the math in local space
            Vector3 expectedLocalChange = CalculateVelocityChange(localForce, _rb.mass, mode);
            Vector3 newLocalVelocity = SoftClamp(localVelocity, expectedLocalChange, limited, min, max);
            // Debug.Log($"Current local vel is {localVelocity:F8}, Expected local change is {expectedLocalChange:F8}, Clamped new local velocity is {newLocalVelocity:F8}");

            // Convert back to worldspace
            Vector3 newWorldVelocity = rotation * newLocalVelocity;
            Vector3 worldChange = newWorldVelocity - worldVelocity;

            _rb.SetLinearVelocity(newWorldVelocity);
            return worldChange;
        }


        Vector3 AddForceWithLocalAxisLimit(Vector3 worldForce, in Bool3 limited, in Vector3 min, in Vector3 max, ForceMode mode = ForceMode.Force) {
            Vector3 worldVelocity = _rb.GetLinearVelocity();

            // Convert everything to local
            Quaternion inverseRot = Quaternion.Inverse(_rb.rotation);
            Vector3 localForce = inverseRot * worldForce;
            Vector3 localVelocity = inverseRot * worldVelocity;

            // Do the math in local space
            Vector3 expectedLocalChange = CalculateVelocityChange(localForce, _rb.mass, mode);
            Vector3 newLocalVelocity = SoftClamp(localVelocity, expectedLocalChange, limited, min, max);
            // print($"Clamped new local velocity is {newLocalVelocity}");

            // Convert back to worldspace
            Vector3 newWorldVelocity = _rb.rotation * newLocalVelocity;
            Vector3 worldChange = newWorldVelocity - worldVelocity;

            _rb.SetLinearVelocity(newWorldVelocity);
            return worldChange;
        }


        /**
         * Add a force with local coordinate system speed limits.
         *
         * Return the actual world space velocity change that occurred
         */
        Vector3 AddRelativeForceWithWorldAxisLimit(Vector3 localForce, in Bool3 limited, in Vector3 min, in Vector3 max,
            ForceMode mode = ForceMode.Force) {
            Vector3 expectedChange = CalculateVelocityChange(localForce, _rb.mass, mode);
            Vector3 newLocalVelocity = SoftClamp(LocalVelocity, expectedChange, limited, min, max);

            Vector3 newWorldVelocity = rotation * newLocalVelocity;
            Vector3 change = newWorldVelocity - _rb.GetLinearVelocity();
            _rb.SetLinearVelocity(rotation * newLocalVelocity);

            return change;
        }

        /**
         * Add a force with world-axis speed limits.
         *
         * Return the actual world space velocity change that occurred
         */
        Vector3 AddForceWithWorldAxisLimit(Vector3 force, in Bool3 limited, in Vector3 min, in Vector3 max, ForceMode mode = ForceMode.Force) {
            Vector3 expectedChange = CalculateVelocityChange(force, _rb.mass, mode);
            Vector3 currentVelocity = _rb.GetLinearVelocity();
            Vector3 newVelocity = SoftClamp(currentVelocity, expectedChange, limited, min, max);
            _rb.SetLinearVelocity(newVelocity);

            // Return the actual diff
            return newVelocity - currentVelocity;
        }

        static float DirectionalClamp(float value, float a, float b) {
            if (a > b) {
                // B is smaller
                if (value < b) return b;
                if (value > a) return a;
                return value;
            }

            // A is smaller
            if (value < a) return a;
            if (value > b) return b;
            return value;
        }

        static Vector3 SoftClamp(in Vector3 currentVelocity, in Vector3 expectedChange, in Bool3 limited, in Vector3 min, in Vector3 max) {
            Vector3 newVelocity = currentVelocity;
            for (int i = 0; i < 3; i++) {
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

        Vector3 AddForceWithOmnidirectionalLimit(Vector3 force, float limit, ForceMode mode = ForceMode.Force) {
            Vector3 velocityChange = CalculateVelocityChangeWithSoftLimit(Velocity, force, limit, mode);
            _rb.AddLinearVelocity(velocityChange);
            return velocityChange;
        }

        Vector3 AddRelativeForceWithOmnidirectionalLimit(Vector3 force, float limit, ForceMode mode = ForceMode.Force) {
            Vector3 localChange = CalculateVelocityChangeWithSoftLimit(this.LocalVelocity, force, limit, mode);
            Vector3 worldChange = rotation * localChange;
            _rb.AddLinearVelocity(worldChange);

            return worldChange;
        }

        public void AddForceWithoutLimit(Vector3 force, ForceMode mode = ForceMode.Force) {
            Vector3 velocityChange = CalculateVelocityChange(force, mass, mode);
            _exemptedVelocityChange += velocityChange;
        }

        public void AddRelativeForceWithoutLimit(Vector3 force, ForceMode mode = ForceMode.Force) {
            force = _rb.rotation * force;
            AddForceWithoutLimit(force, mode);
        }

        private Vector3 CalculateVelocityChangeWithSoftLimit(in Vector3 currentVelocity, in Vector3 force,
            float speedLimit, ForceMode mode = ForceMode.Force) {
            // velocityChange is how much we would be adding to the velocity if we were to just add it normally.
            Vector3 velocityChange = CalculateVelocityChange(force, _rb.mass, mode);
            if (speedLimit < 0 || float.IsPositiveInfinity(speedLimit)) {
                // Speed limit < 0 counts as no limit.
                return velocityChange;
            }

            // expected result is what we would end up with if we just added that directly.
            Vector3 expectedResult = currentVelocity + velocityChange;
            Vector3 excess = expectedResult - Vector3.ClampMagnitude(expectedResult, speedLimit);
            float excessSpeed = excess.magnitude;

            Vector3 component = Vector3.Project(velocityChange, excess);


            float reductionAmount = Mathf.Min(excessSpeed, component.magnitude);
            Vector3 adjustedComponent = component - (reductionAmount * expectedResult.normalized);

            // take out the component in the direction of the overspeed and add the adjusted one.
            Vector3 effectiveVelocityChange = velocityChange - component + adjustedComponent;
            return effectiveVelocityChange;
        }

        #endregion
    }
}