using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Layers {
    [Serializable]
    public readonly struct InteractionLayer : IEquatable<InteractionLayer> {
        public readonly int Index;
        public string Name => Index < 0 ? "Invalid Layer" : BetterPhysics.LayerIndexToName(Index);

        public const int MaxLayerCount = 64;
        public const string DefaultLayerName = "Default";
        public const string UnstoppableLayerName = "Unstoppable";
        public const string FeatherLayerName = "Feather";
        
        public static readonly InteractionLayer DefaultLayer = new(0/*, DefaultLayerName*/);
        public static readonly InteractionLayer UnstoppableLayer = new(1/*, UnstoppableLayerName*/);
        public static readonly InteractionLayer FeatherLayer = new(2/*, FeatherLayerName*/);
        public static readonly InteractionLayer InvalidLayer = new(-1);

        public InteractionLayer(int index) {
            Index = index;
        }

        public static InteractionLayer GetOrCreateLayer(string name) {
            if (BetterPhysics.TryGetLayerFromName(name, out InteractionLayer layer)) {
                return layer;
            }

            return BetterPhysicsSettings.Instance.AddLayer(name);
        }

        public static InteractionLayer FromName(string name) {
            return BetterPhysics.LayerFromName(name);
        }

        public static InteractionLayer FromIndex(int index) {
            return BetterPhysics.LayerFromIndex(index);
        }

        public static IEnumerable<InteractionLayer> GetBuiltinLayers() {
            yield return DefaultLayer;
            yield return UnstoppableLayer;
            yield return FeatherLayer;
        }

        public static IEnumerable<string> GetBuiltinLayerNames() {
            yield return DefaultLayerName;
            yield return UnstoppableLayerName;
            yield return FeatherLayerName;
        }

        public static readonly int BuiltinLayerCount = GetBuiltinLayers().Count();

        internal readonly Vector2Int KeyWith(InteractionLayer layerAsReceiver) {
            return new Vector2Int(Index, layerAsReceiver.Index);
        }
        
        public bool Equals(InteractionLayer other) {
            return Index == other.Index;
        }

        public override bool Equals(object obj) {
            return obj is InteractionLayer other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Index/*, Name*/);
        }
        
        public static bool operator ==(InteractionLayer a, InteractionLayer b) {
            return a.Equals(b);
        }

        public static bool operator !=(InteractionLayer a, InteractionLayer b) {
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
                var layer = InteractionLayer.FromName(name);
                mask |= 1L << layer.Index;
            }

            return new PhysicsLayerMask(mask);
        }
    }
}