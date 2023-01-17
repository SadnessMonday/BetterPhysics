using System;
using Unity.Collections;
using UnityEngine;

namespace SadnessMonday.BetterPhysics {
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public class BetterRigidbody : MonoBehaviour
    {
        Rigidbody rb;
        public float HardScalarLimit = 10f;

        public float SoftScalarLimit = 5f;

        [Tooltip("Per-axis limits. Negative numbers mean unlimited")]
        public Vector3 SoftVectorLimit = -Vector3.one;
        [Tooltip("Per-axis limits. Negative numbers mean unlimited")]
        public Vector3 HardVectorLimit = -Vector3.one;

        public LimitType SoftLimitType;
        public LimitType HardLimitType;

        public Vector3 Velocity { get => rb.velocity; set => rb.velocity = value; }
        public Vector3 LocalVelocity {
            get => Quaternion.Inverse(rb.rotation) * rb.velocity;
            set {
                rb.velocity = rb.rotation * value;
            }
        }
        public float Speed => rb.velocity.magnitude;

        void Awake() {
            rb = GetComponent<Rigidbody>();

            // TODO maybe this should actually happen elsewhere
            foreach (Collider c in GetComponentsInChildren<Collider>()) {
                c.hasModifiableContacts = true;
            }
        }

        #region Rigidbody property pass-through

        public float angularDrag { get => rb.angularDrag; set => rb.angularDrag = value; }
        public Vector3 angularVelocity { get => rb.angularVelocity; set => rb.angularVelocity = value; }
        public Vector3 centerOfMass {get => rb.centerOfMass; set => rb.centerOfMass = value; }
        public CollisionDetectionMode collisionDetectionMode { get => rb.collisionDetectionMode; set => rb.collisionDetectionMode = value; }
        public RigidbodyConstraints constraints { get => rb.constraints; set => rb.constraints = value; }
        public bool detectCollisions { get => rb.detectCollisions; set => rb.detectCollisions = value; }
        public float drag { get => rb.drag; set => rb.drag = value; } // TODO we should probably not use normal RB drag as it won't play well with our stuff?
        public bool freezeRotation { get => rb.freezeRotation; set => rb.freezeRotation = value; }
        public Vector3 inertiaTensor { get => rb.inertiaTensor; set => rb.inertiaTensor = value; }
        public Quaternion inertiaTensorRotation { get => rb.inertiaTensorRotation; set => rb.inertiaTensorRotation = value; }
        public RigidbodyInterpolation interpolation { get => rb.interpolation; set => rb.interpolation = value; }
        public bool isKinematic { get => rb.isKinematic; set => rb.isKinematic = value; }
        public float mass { get => rb.mass; set => rb.mass = value; }
        public float maxAngularVelocity { get => rb.maxAngularVelocity; set => rb.maxAngularVelocity = value; }
        public float maxDepenetrationVelocity { get => rb.maxDepenetrationVelocity; set => rb.maxDepenetrationVelocity = value; }
        public Vector3 position { get => rb.position; set => rb.position = value; }
        public Quaternion rotation { get => rb.rotation; set => rb.rotation = value; }
        public float sleepThreshold { get => rb.sleepThreshold; set => rb.sleepThreshold = value; }
        public int solverIterations { get => rb.solverIterations; set => rb.solverIterations = value; }
        public int solverVelocityIterations { get => rb.solverVelocityIterations; set => rb.solverVelocityIterations = value; }
        public bool useGravity { get => rb.useGravity; set => rb.useGravity = value; } // TODO should we hijack gravity for our own nefarious purposes?
        public Vector3 velocity { get => rb.velocity; set => rb.velocity = value; }
        public Vector3 worldCenterOfMass => rb.worldCenterOfMass;
        
        #endregion

        #region Method pass-through

        // TODO we probably want to do a better passthrough here.
        public void AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius, ForceMode mode = ForceMode.Force) {
            Vector3 myPos = this.position;
            
            // Remap
            float distance = Vector3.Distance(myPos, explosionPosition);
            float interpolant = Mathf.InverseLerp(0, explosionRadius, distance);
            float forceAmount = Mathf.Lerp(explosionForce, 0, interpolant);
            
            Vector3 direction = (myPos - explosionPosition).normalized;

            this.AddForceWithoutLimit(direction * forceAmount, mode);
        }

        public void AddForce(float x, float y, float z, ForceMode mode = ForceMode.Force) {
            // print("Adding force");
            this.AddForce(new Vector3(x, y, z), mode);
        }

        // TODO AddForceAtPosition

        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force) {
            if (SoftLimitType == LimitType.Omnidirectional) {
                AddForceWithSoftLimit(force, SoftLimitType, SoftScalarLimit, mode);
            }
            else {
                AddForceWithSoftLimit(force, SoftLimitType, SoftVectorLimit, mode);
            }

            if (HardLimitType == LimitType.Omnidirectional) {
                ApplyHardLimit(HardLimitType, this.HardScalarLimit);
            }
            else {
                ApplyHardLimit(HardLimitType, this.HardVectorLimit);
            }
        }

        public void AddRelativeForce(Vector3 force, ForceMode mode = ForceMode.Force) {
            // print("Adding relative force");
            if (SoftLimitType == LimitType.Omnidirectional) {
                AddRelativeForceWithSoftLimit(force, SoftLimitType, SoftScalarLimit, mode);
            }
            else {
                AddRelativeForceWithSoftLimit(force, SoftLimitType, SoftVectorLimit, mode);
            }

            if (HardLimitType == LimitType.Omnidirectional) {
                ApplyHardLimit(HardLimitType, this.HardScalarLimit);
            }
            else {
                ApplyHardLimit(HardLimitType, this.HardVectorLimit);
            }
        }

        public void AddRelativeForce(float x, float y, float z, ForceMode mode = ForceMode.Force) {
            AddRelativeForce(new Vector3(x, y, z), mode);
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

        public bool SweepTest(Vector3 direction, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
            return rb.SweepTest(direction, out hitInfo, maxDistance, queryTriggerInteraction);
        }

        public RaycastHit[] SweepTestAll(Vector3 direction, float maxDistance = Mathf.Infinity, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
            return rb.SweepTestAll(direction, maxDistance, queryTriggerInteraction);
        }

        public Action WakeUp => rb.WakeUp;

        #endregion

        #region Unity Messages

        void OnValidate() {
            if (!TryGetComponent(out rb)) {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            
            // Don't show the real Rigidbody in the inspector!
            rb.hideFlags = HideFlags.HideInInspector;
        }

        void OnEnable() {
            Physics.ContactModifyEvent += ModifyContacts;
        }

        void OnDisable() {
            Physics.ContactModifyEvent -= ModifyContacts;
        }

        void ModifyContacts(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs) {
            var pair = pairs[0];
            // pair.SetMaxImpulse
        }

        #endregion

        public void AddForceWithSoftLimit(Vector3 force, LimitType limitType, Vector3 limit, ForceMode mode = ForceMode.Force) {
            switch (limitType) {
                case LimitType.None:
                    AddForceWithoutLimit(force, mode);
                    break;
                case LimitType.Omnidirectional:   
                    throw new Exception($"You have provided a Vector3 limit parameter, which is not compatible with the {limitType} limit type. Please provide a float limit instead");
                case LimitType.WorldAxes:
                    AddForceWithWorldAxisLimit(force, limit, mode);
                    break;
                case LimitType.LocalAxes:
                    AddForceWithLocalAxisLimit(force, limit, mode);
                    break;
            }
        }

        public void AddForceWithSoftLimit(Vector3 force, LimitType limitType, float limit, ForceMode mode = ForceMode.Force) {
            switch (limitType) {
                case LimitType.None:
                    AddForceWithoutLimit(force, mode);
                    break;
                case LimitType.Omnidirectional:
                    AddForceWithOmnidirectionalLimit(force, limit, mode);
                    break;
                case LimitType.WorldAxes:                    
                    throw new Exception($"You have provided a float limit parameter, which is not compatible with the {limitType} limit type. Please provide a Vector3 limit instead");
                case LimitType.LocalAxes:                    
                    throw new Exception($"You have provided a float limit parameter, which is not compatible with the {limitType} limit type. Please provide a Vector3 limit instead");
            }
        }

        public void AddRelativeForceWithSoftLimit(Vector3 force, LimitType limitType, Vector3 limit, ForceMode mode = ForceMode.Force) {
            switch (limitType) {
                case LimitType.None:
                    AddRelativeForceWithoutLimit(force, mode);
                    break;
                case LimitType.Omnidirectional:   
                    throw new Exception($"You have provided a Vector3 limit parameter, which is not compatible with the {limitType} limit type. Please provide a float limit instead");
                case LimitType.WorldAxes:
                    AddRelativeForceWithWorldAxisLimit(force, limit, mode);
                    break;
                case LimitType.LocalAxes:
                    AddRelativeForceWithLocalAxisLimit(force, limit, mode);
                    break;
            }
        }


        public void AddRelativeForceWithSoftLimit(Vector3 force, LimitType limitType, float limit, ForceMode mode = ForceMode.Force) {
            switch (limitType) {
                case LimitType.None:
                    AddRelativeForceWithoutLimit(force, mode);
                    break;
                case LimitType.Omnidirectional:
                    AddRelativeForceWithOmnidirectionalLimit(force, limit, mode);
                    break;
                case LimitType.WorldAxes:                    
                    throw new Exception($"You have provided a float limit parameter, which is not compatible with the {limitType} limit type. Please provide a Vector3 limit instead");
                case LimitType.LocalAxes:                    
                    throw new Exception($"You have provided a float limit parameter, which is not compatible with the {limitType} limit type. Please provide a Vector3 limit instead");
            }
        }


        public void AddRelativeForceWithLocalAxisLimit(Vector3 localForce, Vector3 limit, ForceMode mode = ForceMode.Force) {
            // Convert everything to local
            Vector3 localVelocity = LocalVelocity;

            // Do the math in local space
            Vector3 expectedLocalChange = RigidbodyExtensions.CalculateVelocityChange(localForce, rb.mass, mode);
            Vector3 newLocalVelocity = SoftClamp(localVelocity, expectedLocalChange, limit);
            print($"Clamped new local velocity is {newLocalVelocity}");

            // Convert back to worldspace
            Vector3 newWorldVelocity = rotation * newLocalVelocity;
            rb.velocity = newWorldVelocity;
            print($"After conversion back it is now {LocalVelocity}");
        }


        public void AddForceWithLocalAxisLimit(Vector3 worldForce, Vector3 limit, ForceMode mode = ForceMode.Force) {
            #if UNITY_EDITOR
            if (!Time.inFixedTimeStep) {
                throw new Exception("Forces should only be added during the fixed timestep. This means FixedUpdate, a collision callback method such as OnCollisionEnter, or a coroutine with yield return new WaitForFixedUpdate");
            }
            #endif

            // Convert everything to local
            Quaternion inverseRot = Quaternion.Inverse(rb.rotation);
            Vector3 localForce = inverseRot * worldForce;
            Vector3 localVelocity = inverseRot * rb.velocity;

            // Do the math in local space
            Vector3 expectedLocalChange = RigidbodyExtensions.CalculateVelocityChange(localForce, rb.mass, mode);
            Vector3 newLocalVelocity = SoftClamp(localVelocity, expectedLocalChange, limit);
            print($"Clamped new local velocity is {newLocalVelocity}");

            // Convert back to worldspace
            Vector3 newWorldVelocity = rb.rotation * newLocalVelocity;
            rb.velocity = newWorldVelocity;
            print($"After conversion back it is now {LocalVelocity}");
        }

        public void AddRelativeForceWithWorldAxisLimit(Vector3 localForce, Vector3 limit, ForceMode mode = ForceMode.Force) {
            #if UNITY_EDITOR
            if (!Time.inFixedTimeStep) {
                throw new Exception("Forces should only be added during the fixed timestep. This means FixedUpdate, a collision callback method such as OnCollisionEnter, or a coroutine with yield return new WaitForFixedUpdate");
            }
            #endif

            Vector3 expectedChange = RigidbodyExtensions.CalculateVelocityChange(localForce, rb.mass, mode);
            Vector3 newLocalVelocity = SoftClamp(LocalVelocity, expectedChange, limit);
            rb.velocity = rotation * newLocalVelocity;
        }

        public void AddForceWithWorldAxisLimit(Vector3 force, Vector3 limit, ForceMode mode = ForceMode.Force) {
            #if UNITY_EDITOR
            if (!Time.inFixedTimeStep) {
                throw new Exception("Forces should only be added during the fixed timestep. This means FixedUpdate, a collision callback method such as OnCollisionEnter, or a coroutine with yield return new WaitForFixedUpdate");
            }
            #endif

            Vector3 expectedChange = RigidbodyExtensions.CalculateVelocityChange(force, rb.mass, mode);
            Vector3 currentVelocity = rb.velocity;
            rb.velocity = SoftClamp(currentVelocity, expectedChange, limit);
        }

        static Vector3 SoftClamp(Vector3 currentVelocity, Vector3 expectedChange, Vector3 limits) {
            Vector3 newVelocity = currentVelocity;
            for (int i = 0; i < 3; i++) {
                float axisLimit = limits[i];
                // for each axis...
                float currentAxisVelocity = currentVelocity[i];
                float expectedAxisChange = expectedChange[i];
                float expectedNewVelocity = expectedAxisChange + currentAxisVelocity;
                if (axisLimit < 0) {
                    // less than 0 is no limit
                    newVelocity[i] = expectedNewVelocity;
                    continue;
                }

                float sign = Mathf.Sign(expectedAxisChange);
                if (Mathf.Abs(expectedNewVelocity) > axisLimit && sign == Mathf.Sign(currentAxisVelocity)) {
                   newVelocity[i] = axisLimit * sign;
                }
                else {
                    newVelocity[i] = expectedNewVelocity;
                }
            }

            return newVelocity;
        }

        void AddForceWithOmnidirectionalLimit(Vector3 force, float limit, ForceMode mode = ForceMode.Force) {
            #if UNITY_EDITOR
            if (!Time.inFixedTimeStep) {
                throw new Exception("Forces should only be added during the fixed timestep. This means FixedUpdate, a collision callback method such as OnCollisionEnter, or a coroutine with yield return new WaitForFixedUpdate");
            }
            #endif

            Vector3 velocityChange = CalculateVelocityChangeWithSoftLimit(this.Velocity, force, limit, mode);
            rb.velocity += velocityChange;
        }

        void AddRelativeForceWithOmnidirectionalLimit(Vector3 force, float limit, ForceMode mode = ForceMode.Force) {
            #if UNITY_EDITOR
            if (!Time.inFixedTimeStep) {
                throw new Exception("Forces should only be added during the fixed timestep. This means FixedUpdate, a collision callback method such as OnCollisionEnter, or a coroutine with yield return new WaitForFixedUpdate");
            }
            #endif

            Vector3 localChange = CalculateVelocityChangeWithSoftLimit(this.LocalVelocity, force, limit, mode);
            rb.velocity += rotation * localChange;
        }

        public void AddForceWithoutLimit(Vector3 force, ForceMode mode = ForceMode.Force) {
            #if UNITY_EDITOR
            if (!Time.inFixedTimeStep) {
                throw new Exception("Forces should only be added during the fixed timestep. This means FixedUpdate, a collision callback method such as OnCollisionEnter, or a coroutine with yield return new WaitForFixedUpdate");
            }
            #endif

            rb.VisibleAddForce(force, mode);
        }

        public void AddRelativeForceWithoutLimit(Vector3 force, ForceMode mode = ForceMode.Force) {
            #if UNITY_EDITOR
            if (!Time.inFixedTimeStep) {
                throw new Exception("Forces should only be added during the fixed timestep. This means FixedUpdate, a collision callback method such as OnCollisionEnter, or a coroutine with yield return new WaitForFixedUpdate");
            }
            #endif

            rb.VisibleAddRelativeForce(force, mode);
        }

        public void ApplyHardLimit(LimitType limitType, float speedLimit) {
            switch (limitType) {
                case LimitType.None:
                    return;
                case LimitType.Omnidirectional:
                    rb.velocity = Vector3.ClampMagnitude(rb.velocity, speedLimit);
                    break;
                case LimitType.WorldAxes:
                    throw new Exception($"You have provided a float limit parameter, which is not compatible with the {limitType} limit type. Please provide a Vector3 limit instead");
                case LimitType.LocalAxes:                    
                    throw new Exception($"You have provided a float limit parameter, which is not compatible with the {limitType} limit type. Please provide a Vector3 limit instead");
            }
        }

        public void ApplyHardLimit(LimitType limitType, Vector3 perAxisLimits) {
            switch (limitType) {
                case LimitType.None:
                    return;
                case LimitType.Omnidirectional:                    
                    throw new Exception($"You have provided a Vector3 limit parameter, which is not compatible with the {limitType} limit type. Please provide a float limit instead");
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

        private Vector3 CalculateVelocityChangeWithSoftLimit(in Vector3 currentVelocity, in Vector3 force, float speedLimit, ForceMode mode = ForceMode.Force) {
            float currentSpeed = currentVelocity.magnitude;

            // velocityChange is how much we would be adding to the velocity if we were to just add it normally.
            Vector3 velocityChange = RigidbodyExtensions.CalculateVelocityChange(force, rb.mass, mode);

            // expected result is what we would end up with if we just added that directly.
            Vector3 expectedResult = currentVelocity + velocityChange;
            float expectedSpeed = expectedResult.magnitude;

            Vector3 excess = expectedResult - Vector3.ClampMagnitude(expectedResult, speedLimit);
            float excessSpeed = excess.magnitude;

            Vector3 component = Vector3.Project(velocityChange, excess);


            float reductionAmount = Mathf.Min(excessSpeed, component.magnitude);
            Vector3 adjustedComponent = component - (reductionAmount * expectedResult.normalized);

            // take out the component in the direction of the overspeed and add the adjusted one.
            Vector3 effectiveVelocityChange = velocityChange - component + adjustedComponent;
            return effectiveVelocityChange;
        }
    }
}
