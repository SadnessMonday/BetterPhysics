using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SadnessMonday.BetterPhysics
{
    public class Speedometer : MonoBehaviour
    {
        BetterRigidbody brb;
        void Awake() {
            brb = FindObjectOfType<BetterRigidbody>();
        }

        void OnGUI() {
            GUI.Label(new Rect(10, 10, 500, 20), $"Speed: {brb.Speed:F4}");
            GUI.Label(new Rect(10, 30, 500, 20), $"Local Velocity: {brb.LocalVelocity.ToString("F4")}");
            GUI.Label(new Rect(10, 50, 500, 20), $"World Velocity: {brb.Velocity:F4}");

            // print($"Speed: {brb.Speed:F4} Local Velocity: {brb.LocalVelocity.ToString("F4")} World Velocity: {brb.Velocity:F4}");
        }
    }
}
