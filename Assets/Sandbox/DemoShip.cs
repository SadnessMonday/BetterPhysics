using SadnessMonday.BetterPhysics;
using UnityEngine;

public class DemoShip : MonoBehaviour {
    private BetterRigidbody brb;
    [SerializeField] private float speed = 4f;

    private void Awake() {
        brb = GetComponent<BetterRigidbody>();
    }

    private void Start() {
        brb.AddRelativeForceWithoutLimit(Vector3.forward * speed, ForceMode.VelocityChange);
    }
}
