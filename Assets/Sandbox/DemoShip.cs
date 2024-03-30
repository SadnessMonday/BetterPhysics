using UnityEngine;

public class DemoShip : MonoBehaviour {
    private Rigidbody rb;
    [SerializeField] private float speed = 4f;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void Start() {
        rb.AddForce(Vector3.forward * speed, ForceMode.VelocityChange);
    }
}
