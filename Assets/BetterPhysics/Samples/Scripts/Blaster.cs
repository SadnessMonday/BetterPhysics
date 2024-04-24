using System.Collections.Generic;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Samples {
    public class Blaster : MonoBehaviour {
        [SerializeField] private float force = 10;
        [SerializeField] private ForceMode forceMode = ForceMode.Impulse;
        private HashSet<Collider> alreadyKnown = new();
        
        private void OnTriggerEnter(Collider other) {
            if (!alreadyKnown.Add(other)) return; // don't add force twice to the same object
            
            if (other.TryGetComponent(out BetterRigidbody brb)) {
                Vector3 direction = brb.position - transform.position;
                direction.Normalize();
                brb.AddForce(direction * force, forceMode);
            }
        }

        private void OnTriggerExit(Collider other) {
            alreadyKnown.Remove(other);
        }
    }
}
