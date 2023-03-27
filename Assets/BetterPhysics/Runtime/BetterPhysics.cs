using Palmmedia.ReportGenerator.Core;
using SadnessMonday.BetterPhysics.Layers;

namespace SadnessMonday.BetterPhysics {
    public class BetterPhysics {
        public const string BetterPhysicsVersion = "0.0.1";

        public static int DefinedLayerCount => BetterPhysicsSettings.Instance.DefinedLayerCount;
        
        public static LayerInteraction.InteractionType GetLayerInteraction(int actor, int receiver) {
            BetterPhysicsSettings.Instance.TryGetLayerInteraction(actor, receiver, out LayerInteraction result);
            return result.interactionType;
        }

        public static void SetLayerInteraction(int actor, int receiver,
            LayerInteraction.InteractionType interactionType) {
            BetterPhysicsSettings.Instance.SetLayerIteraction(actor, receiver, interactionType);
        }

        public static string LayerIndexToName(int layerIndex) {
            return BetterPhysicsSettings.Instance.LayerIndexToName(layerIndex);
        }

        public static int LayerNameToIndex(string layerName) {
            return BetterPhysicsSettings.Instance.LayerNameToIndex(layerName);
        }
    }
}