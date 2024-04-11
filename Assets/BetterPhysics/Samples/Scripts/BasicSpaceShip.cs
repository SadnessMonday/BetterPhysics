using System;
using UnityEngine;

public class BasicSpaceShip : MonoBehaviour {
    private Rigidbody rb;

    private void FixedUpdate() {
        float thrust = 0;
        
        float roll = 0;
        float yaw = 0;
        float pitch = 0;

        thrust += Input.GetKey(KeyCode.Space) ? 1 : 0;
    }
}
