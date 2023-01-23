using System;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Utilities {
    [RequireComponent(typeof(BetterRigidbody))]
    public class ColorByPhysicsLayer : MonoBehaviour {
        private BetterRigidbody _brb;
        private Renderer _renderer;

        static readonly Color[] Colors = {
            Color.red,
            Color.blue, 
            Color.green,
            Color.magenta,
            Color.yellow,
            Color.cyan,
        };

        private void Awake() {
            _brb = GetComponentInParent<BetterRigidbody>();
            _renderer = GetComponentInChildren<Renderer>();
        }

        private void OnEnable() {
            UpdateColor(Colors[_brb.PhysicsLayer]);
            _brb.OnPhysicsLayerChanged += WhenPhysicsLayerChanges;
        }

        private void OnDisable() {
            _brb.OnPhysicsLayerChanged -= WhenPhysicsLayerChanges;
        }

        private void WhenPhysicsLayerChanges(BetterRigidbody source, int oldlayer, int newlayer) {
            UpdateColor(Colors[newlayer]);
        }

        void UpdateColor(Color c) {
            _renderer.material.color = c;
        }
    }
}