using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SadnessMonday.BetterPhysics.Layers {
    [Serializable]
    public struct InteractionConfiguration {
        public readonly InteractionLayer actor;
        public readonly InteractionLayer receiver;
        public InteractionType interactionType;

        public InteractionConfiguration(InteractionLayer actor, InteractionLayer receiver, InteractionType interactionType = InteractionType.Default) {
            this.actor = actor;
            this.receiver = receiver;
            this.interactionType = interactionType;
        }

        public static InteractionConfiguration CreateKinematicInteraction(InteractionLayer kinematicLayer, InteractionLayer dynamicLayer) {
            return new InteractionConfiguration(kinematicLayer, dynamicLayer, InteractionType.Kinematic);
        }
        
        public void ResetToDefault() {
            interactionType = InteractionType.Default;
        }

        internal Vector2Int Key() {
            return actor.KeyWith(receiver);
        }
        
    }

    [Serializable]
    public struct OneWayLayerInteraction {
        public int receiver;
        public InteractionType interactionType;

        public OneWayLayerInteraction(int receiver) {
            this.receiver = receiver;

            interactionType = InteractionType.Default;
        }
        
        public void ResetToDefault() {
            interactionType = InteractionType.Default;
        }
    }
}