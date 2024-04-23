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

        [SerializeField] 
        private string formatStr = "{0:F2} m/s";
        
        void Awake() {
            if (brb == null) brb = GetComponent<BetterRigidbody>();
        }

        void LateUpdate() {
            if (speed) speed.text = string.Format(formatStr, brb.velocity.magnitude);
            if (localVelocity) localVelocity.text = $"{brb.LocalVelocity:F2}";
            if (worldVelocity) worldVelocity.text = $"{brb.Velocity:F2}";
        }
    }
}
