using System;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Utilities {
    [RequireComponent(typeof(BetterRigidbody))]
    public class ColorByPhysicsLayer : MonoBehaviour {
        private BetterRigidbody _brb;
        private Renderer _renderer;

        public Color[] colors = {
            Color.red,
            Color.blue, 
            Color.green,
            Color.magenta, 
        };

        private void Awake() {
            _brb = GetComponentInParent<BetterRigidbody>();
            _renderer = GetComponentInChildren<Renderer>();
        }

        private void OnEnable() {
            UpdateColor(colors[_brb.PhysicsLayer]);
            _brb.OnPhysicsLayerChanged += WhenPhysicsLayerChanges;
        }

        private void OnDisable() {
            _brb.OnPhysicsLayerChanged -= WhenPhysicsLayerChanges;
        }

        private void WhenPhysicsLayerChanges(BetterRigidbody source, int oldlayer, int newlayer) {
            UpdateColor(colors[newlayer]);
        }

        void UpdateColor(Color c) {
            _renderer.material.color = c;
        }
    }
}