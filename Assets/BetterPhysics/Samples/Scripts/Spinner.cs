using UnityEngine;

namespace SadnessMonday.BetterPhysics.Samples {
    [RequireComponent(typeof(BetterRigidbody))]
    public class Spinner : MonoBehaviour
    {
        [SerializeField]
        Vector3 RotationAxis = Vector3.forward;
        [SerializeField]
        [Tooltip("Degrees per second")]
        float speed = 180f;

        BetterRigidbody brb;

        void Awake() {
            brb = GetComponent<BetterRigidbody>();
        }
        
        void FixedUpdate() {
            brb.MoveRotation(brb.rotation * Quaternion.Euler(speed * Time.fixedDeltaTime * RotationAxis.normalized));
        }
    } 
}