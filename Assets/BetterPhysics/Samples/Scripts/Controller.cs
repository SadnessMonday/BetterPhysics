using UnityEngine;
#if ENABLE_INPUT_SYSTEM && NEW_INPUT_SYSTEM_INSTALLED
using UnityEngine.InputSystem;
#endif

namespace SadnessMonday.BetterPhysics.Samples {
    [RequireComponent(typeof(BetterRigidbody))]
    public class Controller : MonoBehaviour
    {
        BetterRigidbody brb;
        public float ForceMultiplier = 5f;
        public float RotateSpeed = 50f;
        public bool UseLocalForces = false;
        public bool Flat = false;

        void Awake() {
            brb = GetComponent<BetterRigidbody>();
        }

        Vector3 moveInput;
        float rotateInput;

        // Update is called once per frame
        void Update(){
            moveInput = Vector2.zero;
            rotateInput = 0;

#if ENABLE_INPUT_SYSTEM && NEW_INPUT_SYSTEM_INSTALLED
            Keyboard k = Keyboard.current;
            if (k[Key.W].IsPressed()) moveInput += Vector3.forward;
            if (k[Key.A].IsPressed()) moveInput += Vector3.left;
            if (k[Key.S].IsPressed()) moveInput += Vector3.back;
            if (k[Key.D].IsPressed()) moveInput += Vector3.right;

            if (k[Key.O].IsPressed()) rotateInput -= 1;
            if (k[Key.P].IsPressed()) rotateInput += 1;
#else
            if (Input.GetKey(KeyCode.W)) moveInput += Flat ? Vector3.forward : Vector3.up;
            if (Input.GetKey(KeyCode.A)) moveInput += Vector3.left;
            if (Input.GetKey(KeyCode.S)) moveInput += Flat ? Vector3.back : Vector3.down;
            if (Input.GetKey(KeyCode.D)) moveInput += Vector3.right;

            if (Input.GetKey(KeyCode.O)) rotateInput -= 1;
            if (Input.GetKey(KeyCode.P)) rotateInput += 1;
#endif
        }

        void FixedUpdate() {
            if (UseLocalForces) {
                brb.AddRelativeForce(moveInput * ForceMultiplier);
            }
            else {
                brb.AddForce(moveInput * ForceMultiplier);
            }

            brb.MoveRotation(brb.rotation * Quaternion.Euler(0, 0, rotateInput * RotateSpeed));
        }
    }
}
