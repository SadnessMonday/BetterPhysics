using UnityEngine;
#if ENABLE_INPUT_SYSTEM && NEW_INPUT_SYSTEM_INSTALLED
using UnityEngine.InputSystem;
#endif

namespace SadnessMonday.BetterPhysics.Samples {

    [RequireComponent(typeof(Rigidbody))]
    public class BasicSpaceShip : MonoBehaviour {
        [SerializeField] private float thrustForce = 5;
        [SerializeField] private float pitchTorque = 4;
        [SerializeField] private float rollTorque = 4;
        [SerializeField] private float yawTorque = 4;
        [SerializeField] private ParticleSystem thrustParticles;
        private const float ParticleEmissionMax = 45;
        private const float ParticleEmissionAcceleration = 100f;
        
        private Rigidbody rb;

        private void Awake() {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate() {
            HandleInput(out float thrust, out float pitch, out float roll, out float yaw);
            AddControlForces(thrust, pitch, roll, yaw);

            UpdateThrustParticles(thrust);
        }
        
        private void AddControlForces(float thrust, float pitch, float roll, float yaw) {
            Vector3 force = thrustForce * thrust * Vector3.forward;
            rb.AddRelativeForce(force);

            Vector3 torque = new(
                pitch * pitchTorque,
                yaw * yawTorque,
                roll * rollTorque);
            rb.AddRelativeTorque(torque);
        }
        
        void HandleInput(out float thrust, out float pitch, out float roll, out float yaw) {
            thrust = pitch = roll = yaw = 0;
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKey(KeyCode.LeftShift)) thrust -= 1;
            if (Input.GetKey(KeyCode.Space)) thrust += 1;

            if (Input.GetKey(KeyCode.S)) pitch -= 1;
            if (Input.GetKey(KeyCode.W)) pitch += 1;

            if (Input.GetKey(KeyCode.D)) roll -= 1;
            if (Input.GetKey(KeyCode.A)) roll += 1;

            if (Input.GetKey(KeyCode.Q)) yaw -= 1;
            if (Input.GetKey(KeyCode.E)) yaw += 1;
#elif ENABLE_INPUT_SYSTEM && NEW_INPUT_SYSTEM_INSTALLED
            Keyboard k = Keyboard.current;
            if (k.leftShiftKey.isPressed) thrust -= 1;
            if (k.spaceKey.isPressed) thrust += 1;
            
            if (k.sKey.isPressed) pitch -= 1;
            if (k.wKey.isPressed) pitch += 1;
            
            if (k.aKey.isPressed) roll -= 1;
            if (k.dKey.isPressed) roll += 1;
            
            if (k.qKey.isPressed) yaw -= 1;
            if (k.eKey.isPressed) yaw += 1;
#else
            Debug.LogWarning("No known input systems are enabled");
#endif
        }

        private void UpdateThrustParticles(float thrustInput) {
            if (!thrustParticles) return;

            var emissionModule = thrustParticles.emission;

            var emissionModuleRateOverTime = emissionModule.rateOverTime;
            float currentEmission = emissionModuleRateOverTime.constant;
            float newEmission = Mathf.MoveTowards(currentEmission, ParticleEmissionMax * thrustInput,
                Time.deltaTime * ParticleEmissionAcceleration);
            emissionModuleRateOverTime.constant = newEmission;

            emissionModule.rateOverTime = emissionModuleRateOverTime;
        }
    }
}
