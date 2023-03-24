using System;
using System.Collections.Generic;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Layers {
    [SerializeField]
    public struct PhysicsLayer : IEquatable<PhysicsLayer> {
        public readonly int Number;
        public readonly string Name;

        public const string DefaultLayerName = "Default";
        public const string UnstoppableLayerName = "Unstoppable";
        public const string FeatherLayerName = "Feather";
        
        public static readonly PhysicsLayer DefaultLayer = new(0, DefaultLayerName);
        public static readonly PhysicsLayer UnstoppableLayer = new(1, UnstoppableLayerName);
        public static readonly PhysicsLayer FeatherLayer = new(2, FeatherLayerName);

        internal PhysicsLayer(int number, string name) {
            Number = number;
            Name = name;
        }

        public static IEnumerable<PhysicsLayer> GetBuiltinLayers() {
            yield return DefaultLayer;
            yield return UnstoppableLayer;
            yield return FeatherLayer;
        }

        public bool Equals(PhysicsLayer other) {
            return Number == other.Number;
        }

        public override bool Equals(object obj) {
            return obj is PhysicsLayer other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Number, Name);
        }
        
        public static bool operator ==(PhysicsLayer a, PhysicsLayer b) {
            return a.Equals(b);
        }

        public static bool operator !=(PhysicsLayer a, PhysicsLayer b) {
            return !(a == b);
        }
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