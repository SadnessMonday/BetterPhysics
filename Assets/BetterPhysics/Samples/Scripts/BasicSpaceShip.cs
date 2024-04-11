using System;
using UnityEngine;

public class BasicSpaceShip : MonoBehaviour {
    private Rigidbody rb;

    private void FixedUpdate() {
        float thrust = 0;
        
        float roll = 0;
        float yaw = 0;
        float pitch = 0;

        if (Input.GetKey(KeyCode.Space)) thrust += 1;
        if (Input.GetKey(KeyCode.LeftShift)) thrust -= 1;
        
        if (Input.GetKey(KeyCode.W)) pitch += 1;
        if (Input.GetKey(KeyCode.S)) pitch -= 1;
        
        if (Input.GetKey(KeyCode.A)) roll -= 1;
        if (Input.GetKey(KeyCode.D)) roll += 1;

        if (Input.GetKey(KeyCode.Q)) yaw -= 1;
        if (Input.GetKey(KeyCode.E)) yaw += 1;
    }
}
