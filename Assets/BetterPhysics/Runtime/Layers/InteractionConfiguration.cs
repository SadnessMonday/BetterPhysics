using System;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Layers {
    [Serializable]
    public struct InteractionConfiguration {
        [field:SerializeField] public InteractionLayer Actor { get; private set; }
        [field:SerializeField] public InteractionLayer Receiver  { get; private set; }
        [field:SerializeField] public InteractionType InteractionType { get; private set; }

        public InteractionConfiguration(InteractionLayer actor, InteractionLayer receiver, InteractionType interactionType = InteractionType.Default) {
            Actor = actor;
            Receiver = receiver;
            InteractionType = interactionType;
            
            Normalize();
        }

        public static InteractionConfiguration CreateKinematicInteraction(InteractionLayer kinematicLayer, InteractionLayer dynamicLayer) {
            return new InteractionConfiguration(kinematicLayer, dynamicLayer, InteractionType.Kinematic);
        }
        
        public void ResetToDefault() {
            InteractionType = InteractionType.Default;
        }

        internal Vector2Int Key() {
            return Actor.KeyWith(Receiver);
        }

        void Normalize() {
            var key = Key();
            if (key.Normalize()) {
                InteractionType = InteractionType.Inverse();
            }
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