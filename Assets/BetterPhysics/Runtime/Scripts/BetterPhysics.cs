using System;
using SadnessMonday.BetterPhysics.Layers;

namespace SadnessMonday.BetterPhysics {
    public class BetterPhysics {
        public const string BetterPhysicsVersion = "1.0.4";

        public static int DefinedLayerCount => BetterPhysicsSettings.Instance.DefinedLayerCount;
        
        public static InteractionType GetLayerInteraction(int actor, int receiver) {
            BetterPhysicsSettings.Instance.TryGetLayerInteraction(actor, receiver, out InteractionConfiguration result);
            return result.InteractionType;
        }

        public static void SetLayerInteraction(InteractionLayer actor, InteractionLayer receiver,
            InteractionType interactionType) {
            BetterPhysicsSettings.Instance.SetLayerInteraction(actor.KeyWith(receiver), interactionType);
        }
        
        internal static void SetLayerInteraction(int actor, int receiver,
            InteractionType interactionType) {
            BetterPhysicsSettings.Instance.SetLayerInteraction(new InteractionLayer(actor), new InteractionLayer(receiver), interactionType);
        }

        public static string LayerIndexToName(int layerIndex) {
            return BetterPhysicsSettings.Instance.LayerIndexToName(layerIndex);
        }

        public static InteractionLayer LayerFromName(string name) {
            return BetterPhysicsSettings.Instance.LayerFromName(name);
        }

        public static bool TryGetLayerFromName(string name, out InteractionLayer layer) {
            return BetterPhysicsSettings.Instance.TryGetLayerFromName(name, out layer);
        }

        public static InteractionLayer LayerFromIndex(int index) {
            if (!BetterPhysicsSettings.Instance.LayerIsDefined(index)) {
                throw new Exception($"Undefined interaction layer {index}");
            }
            InteractionLayer layer = new InteractionLayer(index);
            return layer;
        }
    }

    public class BetterPhysicsException : Exception {
        internal BetterPhysicsException() : base() { }
        internal BetterPhysicsException(string message) : base(message) { }
        internal BetterPhysicsException(string message, Exception innerException) : base(message, innerException) { }
    }
}