using UnityEngine;
using UnityEngine.UI;

namespace SadnessMonday.BetterPhysics.Utilities
{
    public class Speedometer : MonoBehaviour
    {
        [SerializeField]
        BetterRigidbody brb;
        [SerializeField]
        Text speed;
        [SerializeField]
        Text localVelocity;
        [SerializeField]
        Text worldVelocity;
        
        void Awake() {
            if (brb == null) brb = GetComponent<BetterRigidbody>();
        }

        void LateUpdate() {
            if (speed) speed.text = brb.Speed.ToString("F4");
            if (localVelocity) localVelocity.text = brb.LocalVelocity.ToString("F4");
            if (worldVelocity) worldVelocity.text = brb.Velocity.ToString("F4");
        }
    }
}
