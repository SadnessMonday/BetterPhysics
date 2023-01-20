using UnityEngine;

namespace SadnessMonday.BetterPhysics.Layers {
    [SerializeField]
    public struct PhysicsLayer {
        private readonly int _layerNumber;
        private readonly string _name;
    }

    public struct PhysicsLayerMask {
        private readonly long _mask;

        public PhysicsLayerMask(long mask) {
            _mask = mask;
        }

        public static PhysicsLayerMask Create(params string[] layerNames) {
            var settings = BetterPhysicsSettings.Instance;
            
            long mask = 0;
            foreach (var name in layerNames) {
                int index = settings.NameToLayerIndex(name);
                mask |= 1L << index;
            }

            return new PhysicsLayerMask(mask);
        }
    }
}