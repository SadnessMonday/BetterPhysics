using System;
using System.Collections.Generic;
using System.Linq;
using SadnessMonday.BetterPhysics.Layers;
using Unity.Collections;
using UnityEngine;

namespace SadnessMonday.BetterPhysics {
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public class BetterRigidbody : MonoBehaviour {
        public delegate void PhysicsLayerChangeHandler(BetterRigidbody source, int oldLayer, int newLayer);

        public event PhysicsLayerChangeHandler OnPhysicsLayerChanged;

        Rigidbody rb;
        public float hardScalarLimit = 10f;
        public float softScalarLimit = 5f;

        [Tooltip("Per-axis limits. Negative numbers mean unlimited")]
        public Vector3 softVectorLimit = -Vector3.one;

        [Tooltip("Per-axis limits. Negative numbers mean unlimited")]
        public Vector3 hardVectorLimit = -Vector3.one;

        public Limits l;

        internal int GetRigidbodyInstanceID() => rb.GetInstanceID();
        internal Rigidbody WrappedRigidbody => GetComponent<Rigidbody>();

        public LimitType softLimitType;
        public LimitType hardLimitType;

        public Vector3 Velocity {
            get => rb.velocity;
            set => rb.velocity = value;
        }

        public Vector3 LocalVelocity {
            get => Quaternion.Inverse(rb.rotation) * rb.velocity;
            set => rb.velocity = rb.rotation * value;
        }

        public float Speed => rb.velocity.magnitude;

        [SerializeField] private List<OneWayLayerInteraction> layerInteractions = new();

        public bool TryGetInteraction(int receiverLayer, out OneWayLayerInteraction interaction) {
            return ContactModificationManager.Instance.TryGetCustomInteraction(this, receiverLayer, out interaction);
        }

        #region Rigidbody property pass-through

        public float angularDrag {
            get => rb.angularDrag;
            set => rb.angularDrag = value;
        }

        public Vector3 angularVelocity {
            get => rb.angularVelocity;
            set => rb.angularVelocity = value;
        }

        public Vector3 centerOfMass {
            get => rb.centerOfMass;
            set => rb.centerOfMass = value;
        }

        public CollisionDetectionMode collisionDetectionMode {
            get => rb.collisionDetectionMode;
            set => rb.collisionDetectionMode = value;
        }

        public RigidbodyConstraints constraints {
            get => rb.constraints;
            set => rb.constraints = value;
        }

        public bool detectCollisions {
            get => rb.detectCollisions;
            set => rb.detectCollisions = value;
        }

        public float drag {
            get => rb.drag;
            set => rb.drag = value;
        } // TODO we should probably not use normal RB drag as it won't play well with our stuff?

        public bool freezeRotation {
            get => rb.freezeRotation;
            set => rb.freezeRotation = value;
        }

        public Vector3 inertiaTensor {
            get => rb.inertiaTensor;
            set => rb.inertiaTensor = value;
        }

        public Quaternion inertiaTensorRotation {
            get => rb.inertiaTensorRotation;
            set => rb.inertiaTensorRotation = value;
        }

        public RigidbodyInterpolation interpolation {
            get => rb.interpolation;
            set => rb.interpolation = value;
        }

        public bool isKinematic {
            get => rb.isKinematic;
            set => rb.isKinematic = value;
        }

        public float mass {
            get => rb.mass;
            set => rb.mass = value;
        }

        public float maxAngularVelocity {
            get => rb.maxAngularVelocity;
            set => rb.maxAngularVelocity = value;
        }

        public float maxDepenetrationVelocity {
            get => rb.maxDepenetrationVelocity;
            set => rb.maxDepenetrationVelocity = value;
        }

        public Vector3 position {
            get => rb.position;
            set => rb.position = value;
        }

        public Quaternion rotation {
            get => rb.rotation;
            set => rb.rotation = value;
        }

        public float sleepThreshold {
            get => rb.sleepThreshold;
            set => rb.sleepThreshold = value;
        }

        public int solverIterations {
            get => rb.solverIterations;
            set => rb.solverIterations = value;
        }

        public int solverVelocityIterations {
            get => rb.solverVelocityIterations;
            set => rb.solverVelocityIterations = value;
        }

        public bool useGravity {
            get => rb.useGravity;
            set => rb.useGravity = value;
        } // TODO should we hijack gravity for our own nefarious purposes?

        public Vector3 velocity {
            get => rb.velocity;
            set => rb.velocity = value;
        }

        public Vector3 worldCenterOfMass => rb.worldCenterOfMass;

        #endregion

        #region Method pass-through

        // TODO we probably want to do a better passthrough here.
        public Vector3 AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius,
            float upwardsModifier,
            ForceMode mode = ForceMode.Force) {
            Vector3 myPos = this.position;

            // Remap
            float distance = Vector3.Distance(myPos, explosionPosition);
            float interpolant = Mathf.InverseLerp(0, explosionRadius, distance);
            float forceAmount = Mathf.Lerp(explosionForce, 0, interpolant);

            Vector3 direction = myPos - explosionPosition;
            if (upwardsModifier == 0) {
                // normalize
                direction /= distance;
            }
            else {
                direction.y += upwardsModifier;
                direction.Normalize();
            }

            return AddForceWithoutLimit(direction * forceAmount, mode);
        }

        public Vector3 AddForce(float x, float y, float z, ForceMode mode = ForceMode.Force) {
            // print("Adding force");
            return AddForce(new Vector3(x, y, z), mode);
        }

        // TODO AddForceAtPosition

        public Vector3 AddForce(Vector3 force, ForceMode mode = ForceMode.Force) {
            Vector3 velocityChange;
            if (softLimitType == LimitType.Omnidirectional) {
                velocityChange = AddForceWithSoftLimit(force, softLimitType, softScalarLimit, mode);
            }
            else {
                velocityChange = AddForceWithSoftLimit(force, softLimitType, softVectorLimit, mode);
            }

            if (hardLimitType == LimitType.Omnidirectional) {
                ApplyHardLimit(hardLimitType, this.hardScalarLimit);
            }
            else {
                ApplyHardLimit(hardLimitType, this.hardVectorLimit);
            }

            return velocityChange;
        }

        /**
         * Adds the desired force in the body's local coordinate system and returns the absolute effective change in
         * velocity that occurred in world space.
         */
        public Vector3 AddRelativeForce(Vector3 force, ForceMode mode = ForceMode.Force) {
            Vector3 velocityChange;
            // print("Adding relative force");
            if (softLimitType == LimitType.Omnidirectional) {
                velocityChange = AddRelativeForceWithSoftLimit(force, softLimitType, softScalarLimit, mode);
            }
            else {
                velocityChange = AddRelativeForceWithSoftLimit(force, softLimitType, softVectorLimit, mode);
            }

            if (hardLimitType == LimitType.Omnidirectional) {
                ApplyHardLimit(hardLimitType, this.hardScalarLimit);
            }
            else {
                ApplyHardLimit(hardLimitType, this.hardVectorLimit);
            }

            return velocityChange;
        }

        /**
         * Adds the desired force in the body's local coordinate system and returns the absolute effective change in
         * velocity that occurred in world space.
         */
        public Vector3 AddRelativeForce(float x, float y, float z, ForceMode mode = ForceMode.Force) {
            return AddRelativeForce(new Vector3(x, y, z), mode);
        }

        // TODO AddRelativeTorque
        // TODO AddTorque

        public Func<Vector3, Vector3> ClosestPointOnBounds => rb.ClosestPointOnBounds;
        public Func<Vector3, Vector3> GetPointVelocity => rb.GetPointVelocity;
        public Func<Vector3, Vector3> GetRelativePointVelocity => rb.GetRelativePointVelocity;
        public Func<bool> IsSleeping => rb.IsSleeping;
        public Action<Vector3> MovePosition => rb.MovePosition;
        public Action<Quaternion> MoveRotation => rb.MoveRotation;
        public Action ResetCenterOfMass => rb.ResetCenterOfMass;
        public Action ResetInertiaTensor => rb.ResetInertiaTensor;
        public Action<float> SetDensity => rb.SetDensity;
        public Action Sleep => rb.Sleep;

        public bool SweepTest(Vector3 direction, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
            return rb.SweepTest(direction, out hitInfo, maxDistance, queryTriggerInteraction);
        }

        public RaycastHit[] SweepTestAll(Vector3 direction, float maxDistance = Mathf.Infinity,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
            return rb.SweepTestAll(direction, maxDistance, queryTriggerInteraction);
        }

        public Action WakeUp => rb.WakeUp;
        public CustomCollisionData CustomCollisionData { get; set; }

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
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying)
#endif
                {
                    ContactModificationManager.Instance.UpdateBodyLayer(this);
                }

                OnPhysicsLayerChanged?.Invoke(this, oldValue, value);
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
            if (!TryGetComponent(out rb)) {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            if (ReferenceEquals(null, rb)) {
                throw new Exception(
                    "Problem creating BetterRigidbody. There may be incompatible components present, such as 2D physics components");
            }

            rb.hideFlags = HideFlags.HideInInspector;
        }

        void OnValidate() {
            EnsureRigidbody();
            PhysicsLayer = physicsLayer;
        }

        private void OnDestroy() {
            if (rb != null) {
                Destroy(rb);
            }
        }

        void OnEnable() {
            ContactModificationManager.Instance.Register(this);
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

        public Vector3 AddForceWithSoftLimit(Vector3 force, LimitType limitType, Vector3 limit,
            ForceMode mode = ForceMode.Force) {
            switch (limitType) {
                case LimitType.None:
                    return AddForceWithoutLimit(force, mode);
                case LimitType.Omnidirectional:
                    throw new Exception(
                        $"You have provided a Vector3 limit parameter, which is not compatible with the {limitType} limit type. Please provide a float limit instead");
                case LimitType.WorldAxes:
                    return AddForceWithWorldAxisLimit(force, limit, mode);
                case LimitType.LocalAxes:
                    return AddForceWithLocalAxisLimit(force, limit, mode);
                default:
                    throw new NotImplementedException($"Unknown limit type: {limitType.ToString()}");
            }
        }

        public Vector3 AddForceWithSoftLimit(Vector3 force, LimitType limitType, float limit,
            ForceMode mode = ForceMode.Force) {
            switch (limitType) {
                case LimitType.None:
                    return AddForceWithoutLimit(force, mode);
                case LimitType.Omnidirectional:
                    return AddForceWithOmnidirectionalLimit(force, limit, mode);
                case LimitType.WorldAxes:
                    throw new Exception(
                        $"You have provided a float limit parameter, which is not compatible with the {limitType} limit type. Please provide a Vector3 limit instead");
                case LimitType.LocalAxes:
                    throw new Exception(
                        $"You have provided a float limit parameter, which is not compatible with the {limitType} limit type. Please provide a Vector3 limit instead");
                default:
                    throw new NotImplementedException($"Unknown limit type: {limitType.ToString()}");
            }
        }

        public Vector3 AddRelativeForceWithSoftLimit(Vector3 force, LimitType limitType, Vector3 limit,
            ForceMode mode = ForceMode.Force) {
            switch (limitType) {
                case LimitType.None:
                    return AddRelativeForceWithoutLimit(force, mode);
                case LimitType.Omnidirectional:
                    throw new Exception(
                        $"You have provided a Vector3 limit parameter, which is not compatible with the {limitType} limit type. Please provide a float limit instead");
                case LimitType.WorldAxes:
                    return AddRelativeForceWithWorldAxisLimit(force, limit, mode);
                case LimitType.LocalAxes:
                    return AddRelativeForceWithLocalAxisLimit(force, limit, mode);
                default:
                    throw new NotImplementedException($"Unknown limit type: {limitType.ToString()}");
            }
        }


        public Vector3 AddRelativeForceWithSoftLimit(Vector3 force, LimitType limitType, float limit,
            ForceMode mode = ForceMode.Force) {
            switch (limitType) {
                case LimitType.None:
                    return AddRelativeForceWithoutLimit(force, mode);
                case LimitType.Omnidirectional:
                    return AddRelativeForceWithOmnidirectionalLimit(force, limit, mode);
                case LimitType.WorldAxes:
                    throw new Exception(
                        $"You have provided a float limit parameter, which is not compatible with the {limitType} limit type. Please provide a Vector3 limit instead");
                case LimitType.LocalAxes:
                    throw new Exception(
                        $"You have provided a float limit parameter, which is not compatible with the {limitType} limit type. Please provide a Vector3 limit instead");
                default:
                    throw new NotImplementedException($"Unknown limit type: {limitType.ToString()}");
            }
        }


        public Vector3 AddRelativeForceWithLocalAxisLimit(Vector3 localForce, Vector3 limit,
            ForceMode mode = ForceMode.Force) {
            // Convert everything to local
            Vector3 localVelocity = LocalVelocity;
            Vector3 worldVelocity = rb.velocity;

            // Do the math in local space
            Vector3 expectedLocalChange = RigidbodyExtensions.CalculateVelocityChange(localForce, rb.mass, mode);
            Vector3 newLocalVelocity = SoftClamp(localVelocity, expectedLocalChange, limit);
            print(
                $"Current local vel is {localVelocity}, Expected local change is {expectedLocalChange}, Clamped new local velocity is {newLocalVelocity}");

            // Convert back to worldspace
            Vector3 newWorldVelocity = rotation * newLocalVelocity;
            Vector3 worldChange = newWorldVelocity - worldVelocity;

            rb.velocity = newWorldVelocity;
            return worldChange;
        }


        public Vector3 AddForceWithLocalAxisLimit(Vector3 worldForce, Vector3 limit, ForceMode mode = ForceMode.Force) {
#if UNITY_EDITOR && !BETTER_PHYSICS_IGNORE_FIXED_TIMESTEP
            CheckForFixedTimestep();
#endif

            Vector3 worldVelocity = rb.velocity;

            // Convert everything to local
            Quaternion inverseRot = Quaternion.Inverse(rb.rotation);
            Vector3 localForce = inverseRot * worldForce;
            Vector3 localVelocity = inverseRot * worldVelocity;

            // Do the math in local space
            Vector3 expectedLocalChange = RigidbodyExtensions.CalculateVelocityChange(localForce, rb.mass, mode);
            Vector3 newLocalVelocity = SoftClamp(localVelocity, expectedLocalChange, limit);
            // print($"Clamped new local velocity is {newLocalVelocity}");

            // Convert back to worldspace
            Vector3 newWorldVelocity = rb.rotation * newLocalVelocity;
            Vector3 worldChange = newWorldVelocity - worldVelocity;

            rb.velocity = newWorldVelocity;
            return worldChange;
        }


        /**
         * Add a force with local coordinate system speed limits.
         *
         * Return the actual world space velocity change that occurred
         */
        public Vector3 AddRelativeForceWithWorldAxisLimit(Vector3 localForce, Vector3 limit,
            ForceMode mode = ForceMode.Force) {
#if UNITY_EDITOR && !BETTER_PHYSICS_IGNORE_FIXED_TIMESTEP
            CheckForFixedTimestep();
#endif

            Vector3 expectedChange = RigidbodyExtensions.CalculateVelocityChange(localForce, rb.mass, mode);
            Vector3 newLocalVelocity = SoftClamp(LocalVelocity, expectedChange, limit);

            Vector3 newWorldVelocity = rotation * newLocalVelocity;
            Vector3 change = newWorldVelocity - rb.velocity;
            rb.velocity = rotation * newLocalVelocity;

            return change;
        }

        /**
         * Add a force with world-axis speed limits.
         *
         * Return the actual world space velocity change that occurred
         */
        public Vector3 AddForceWithWorldAxisLimit(Vector3 force, Vector3 limit, ForceMode mode = ForceMode.Force) {
#if UNITY_EDITOR && !BETTER_PHYSICS_IGNORE_FIXED_TIMESTEP
            CheckForFixedTimestep();
#endif

            Vector3 expectedChange = RigidbodyExtensions.CalculateVelocityChange(force, rb.mass, mode);
            Vector3 currentVelocity = rb.velocity;
            Vector3 newVelocity = SoftClamp(currentVelocity, expectedChange, limit);
            rb.velocity = newVelocity;

            // Return the actual diff
            return newVelocity - currentVelocity;
        }

        static Vector3 SoftClamp(in Vector3 currentVelocity, in Vector3 expectedChange, in Vector3 limits) {
            Vector3 newVelocity = currentVelocity;
            for (int i = 0; i < 3; i++) {
                float axisLimit = limits[i];
                // for each axis...
                float currentAxisVelocity = currentVelocity[i];
                float expectedAxisChange = expectedChange[i];
                float expectedNewVelocity = expectedAxisChange + currentAxisVelocity;
                int signOfExpectedChange = Math.Sign(expectedAxisChange);
                int expectedSign = Math.Sign(expectedNewVelocity);

                // Less than 0 is no limit
                // Or If expected change is 0, we're just going to have no change
                if (axisLimit < 0 || signOfExpectedChange == 0) {
                    newVelocity[i] = expectedNewVelocity;
                    continue;
                }

                int currentAxisSign = Math.Sign(currentAxisVelocity);
                float currentAxisMagnitude = Mathf.Abs(currentAxisVelocity);
                float expectedMagnitude = Mathf.Abs(expectedNewVelocity);

                // Is the expected speed outside of the limits?
                if (expectedMagnitude > axisLimit) {
                    // was current also outside of the axis limit?
                    if (currentAxisMagnitude > axisLimit) {
                        // did current have same sign as expected?
                        if (currentAxisSign == expectedSign) {
                            // Use whichever out of current/expected is closer to 0
                            newVelocity[i] = expectedSign * Mathf.Min(currentAxisMagnitude, expectedMagnitude);
                        }
                        else {
                            // Use sign(expected) * axisLimit
                            newVelocity[i] = axisLimit * expectedSign;
                        }
                    }
                    else {
                        // Use sign(expected) * axisLimit
                        newVelocity[i] = axisLimit * expectedSign;
                    }
                }
                else {
                    // Use expected speed, since it's within limits
                    newVelocity[i] = expectedNewVelocity;
                }
            }

            // Debug.Log($"curr: {currentVelocity}, expectedChange: {expectedChange}, limits: {limits}, new: {newVelocity}");
            return newVelocity;
        }

        Vector3 AddForceWithOmnidirectionalLimit(Vector3 force, float limit, ForceMode mode = ForceMode.Force) {
#if UNITY_EDITOR && !BETTER_PHYSICS_IGNORE_FIXED_TIMESTEP
            CheckForFixedTimestep();
#endif

            Vector3 velocityChange = CalculateVelocityChangeWithSoftLimit(this.Velocity, force, limit, mode);
            rb.velocity += velocityChange;
            return velocityChange;
        }

        Vector3 AddRelativeForceWithOmnidirectionalLimit(Vector3 force, float limit, ForceMode mode = ForceMode.Force) {
#if UNITY_EDITOR && !BETTER_PHYSICS_IGNORE_FIXED_TIMESTEP
            CheckForFixedTimestep();
#endif

            Vector3 localChange = CalculateVelocityChangeWithSoftLimit(this.LocalVelocity, force, limit, mode);
            Vector3 worldChange = rotation * localChange;
            rb.velocity += worldChange;

            return worldChange;
        }

        public Vector3 AddForceWithoutLimit(Vector3 force, ForceMode mode = ForceMode.Force) {
#if UNITY_EDITOR && !BETTER_PHYSICS_IGNORE_FIXED_TIMESTEP
            CheckForFixedTimestep();
#endif

            return rb.VisibleAddForce(force, mode);
        }

        public Vector3 AddRelativeForceWithoutLimit(Vector3 force, ForceMode mode = ForceMode.Force) {
#if UNITY_EDITOR && !BETTER_PHYSICS_IGNORE_FIXED_TIMESTEP
            CheckForFixedTimestep();
#endif

            return rb.VisibleAddRelativeForce(force, mode);
        }

        public void ApplyHardLimit(LimitType limitType, float speedLimit) {
            switch (limitType) {
                case LimitType.None:
                    return;
                case LimitType.Omnidirectional:
                    rb.velocity = Vector3.ClampMagnitude(rb.velocity, speedLimit);
                    break;
                case LimitType.WorldAxes:
                    throw new Exception(
                        $"You have provided a float limit parameter, which is not compatible with the {limitType} limit type. Please provide a Vector3 limit instead");
                case LimitType.LocalAxes:
                    throw new Exception(
                        $"You have provided a float limit parameter, which is not compatible with the {limitType} limit type. Please provide a Vector3 limit instead");
            }
        }

        public void ApplyHardLimit(LimitType limitType, Vector3 perAxisLimits) {
            switch (limitType) {
                case LimitType.None:
                    return;
                case LimitType.Omnidirectional:
                    throw new Exception(
                        $"You have provided a Vector3 limit parameter, which is not compatible with the {limitType} limit type. Please provide a float limit instead");
                case LimitType.WorldAxes:
                    Vector3 velocity = rb.velocity;
                    for (int i = 0; i < 3; i++) {
                        if (perAxisLimits[i] < 0) continue;
                        velocity[i] = Mathf.Clamp(velocity[i], -perAxisLimits[i], perAxisLimits[i]);
                    }

                    rb.velocity = velocity;
                    break;
                case LimitType.LocalAxes:
                    Vector3 localVelocity = LocalVelocity;
                    for (int i = 0; i < 3; i++) {
                        if (perAxisLimits[i] < 0) continue;
                        localVelocity[i] = Mathf.Clamp(localVelocity[i], -perAxisLimits[i], perAxisLimits[i]);
                    }

                    rb.velocity = rb.rotation * localVelocity;
                    break;
            }
        }

        private Vector3 CalculateVelocityChangeWithSoftLimit(in Vector3 currentVelocity, in Vector3 force,
            float speedLimit, ForceMode mode = ForceMode.Force) {
            // velocityChange is how much we would be adding to the velocity if we were to just add it normally.
            Vector3 velocityChange = RigidbodyExtensions.CalculateVelocityChange(force, rb.mass, mode);
            if (speedLimit < 0) {
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

        private static void CheckForFixedTimestep() {
            if (!Time.inFixedTimeStep) {
                throw new Exception(
                    "Forces should only be added during the fixed timestep. This means FixedUpdate, a collision callback method such as OnCollisionEnter, or a coroutine with yield return new WaitForFixedUpdate");
            }
        }

        #endregion
    }
}