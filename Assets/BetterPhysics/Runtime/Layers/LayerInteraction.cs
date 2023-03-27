using System;
using UnityEngine.Serialization;

namespace SadnessMonday.BetterPhysics.Layers {
    [Serializable]
    public struct LayerInteraction {
        public enum InteractionType : short {
            Default, // the default, unmodified reaction
            Feather, // A feather interaction means the actor will not affect the receiver at all.
            Kinematic // A kinematic interaction means the receiver will not affect the actor at all.
        }

        public readonly int actor;
        public readonly int receiver;
        public InteractionType interactionType;

        public LayerInteraction(int actor, int receiver, InteractionType interactionType = InteractionType.Default) {
            this.actor = actor;
            this.receiver = receiver;
            this.interactionType = interactionType;
        }

        public static LayerInteraction CreateKinematicInteraction(int kinematicLayer, int dynamicLayer) {
            return new LayerInteraction(kinematicLayer, dynamicLayer, InteractionType.Kinematic);
        }
        
        public void ResetToDefault() {
            interactionType = InteractionType.Default;
        }

        public static LayerInteraction CreateKinematicInteraction(PhysicsLayer kinematicLayer, PhysicsLayer dynamicLayer) {
            return CreateKinematicInteraction(kinematicLayer.Number, dynamicLayer.Number);
        }
    }

    [Serializable]
    public struct OneWayLayerInteraction {
        public int receiver;
        public LayerInteraction.InteractionType interactionType;

        public OneWayLayerInteraction(int receiver) {
            this.receiver = receiver;

            interactionType = LayerInteraction.InteractionType.Default;
        }
        
        public void ResetToDefault() {
            interactionType = LayerInteraction.InteractionType.Default;
        }
    }
}