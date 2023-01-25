using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using static UnityEngine.Object;

namespace SadnessMonday.BetterPhysics.Utilities
{
    [RequireComponent(typeof(BetterRigidbody))]
    public class Controller : MonoBehaviour
    {
        BetterRigidbody brb;
        public float ForceMultiplier = 5f;
        public float RotateSpeed = 50f;
        public bool UseLocalForces = false;

        void Awake() {
            brb = GetComponent<BetterRigidbody>();
        }

        // Start is called before the first frame update
        void Start()
        {
            // Application.targetFrameRate = 30;
        }

        Vector3 moveInput;
        float rotateInput;

        // Update is called once per frame
        void Update(){
            moveInput = Vector2.zero;
            rotateInput = 0;

#if ENABLE_INPUT_SYSTEM
            Keyboard k = Keyboard.current;
            if (k[Key.W]) moveInput += Vector3.forward;
            if (k[Key.A]) moveInput += Vector3.left;
            if (k[Key.S]) moveInput += Vector3.back;
            if (k[Key.D]) moveInput += Vector3.right;

            if (k[Key.O]) rotateInput -= 1;
            if (k[Key.P]) rotateInput += 1;
#else
            if (Input.GetKey(KeyCode.W)) moveInput += Vector3.forward;
            if (Input.GetKey(KeyCode.A)) moveInput += Vector3.left;
            if (Input.GetKey(KeyCode.S)) moveInput += Vector3.back;
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
