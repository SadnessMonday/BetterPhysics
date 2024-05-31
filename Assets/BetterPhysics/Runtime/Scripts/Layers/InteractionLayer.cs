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

        public const int DefaultLayerIndex = 0;
        public const int UnstoppableLayerIndex = 1;
        public const int FeatherLayerIndex = 2;
        
        public static readonly InteractionLayer DefaultLayer = new(DefaultLayerIndex);
        public static readonly InteractionLayer UnstoppableLayer = new(UnstoppableLayerIndex);
        public static readonly InteractionLayer FeatherLayer = new(FeatherLayerIndex);
        public static readonly InteractionLayer InvalidLayer = new(-1);

        public InteractionLayer(int index) {
            Index = index;
        }

        public static bool IsReservedLayer(int layerIndex) {
            return layerIndex == UnstoppableLayerIndex || layerIndex == FeatherLayerIndex;
        }
        
        public static bool IncludesReservedLayers(Vector2Int key) {
            return IsReservedLayer(key.x)
                   || IsReservedLayer(key.y);
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
        
        public static implicit operator int(InteractionLayer layer) {
            return layer.Index;
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

    public static class InteractionLayerExtensions {
        public static bool IsReserved(this InteractionLayer layer) {
            return InteractionLayer.IsReservedLayer(layer.Index);
        }

        /**
         * Return true if swapped.
         */
        internal static bool Normalize(ref this Vector2Int key) {
            if (key.x.CompareTo(key.y) > 0) {
                // Swap
                (key.x, key.y) = (key.y, key.x);
                return true;
            }

            return false;
        }

        internal static bool NormalizedCopy(this in Vector2Int key, out Vector2Int normalized) {
            if (key.x.CompareTo(key.y) > 0) {
                // Swap
                normalized = new (key.y, key.x);
                return true;
            }

            normalized = key;
            return false;
        }

        internal static void Normalize(ref this Vector2Int key, ref InteractionType interactionType) {
            if (key.Normalize()) {
                interactionType = interactionType.Inverse();
            }
        }
    }
}