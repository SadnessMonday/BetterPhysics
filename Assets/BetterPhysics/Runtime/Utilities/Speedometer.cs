using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SadnessMonday.BetterPhysics
{
    public class Speedometer : MonoBehaviour
    {
        [SerializeField]
        BetterRigidbody brb;

#if TEXTMESHPRO_PRESENT
        [SerializeField]
        TMPro.TMP_Text speed;
        [SerializeField]
        TMPro.TMP_Text localVelocity;
        [SerializeField]
        TMPro.TMP_Text worldVelocity;
#else
        [SerializeField]
        Text speed;
        [SerializeField]
        Text localVelocity;
        [SerializeField]
        Text worldVelocity;
#endif
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
