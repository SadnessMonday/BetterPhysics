using System;
using UnityEngine.Serialization;

namespace SadnessMonday.BetterPhysics.Layers {
    [Serializable]
    public struct LayerInteraction {
        public const float DefaultImpulseMultiplier = 1f;
        
        public readonly int actor;
        public readonly int receiver;
        public float impulseMultiplier;

        public LayerInteraction(int actor, int receiver, float impulseMultiplier = DefaultImpulseMultiplier) {
            this.actor = actor;
            this.receiver = receiver;
            this.impulseMultiplier = DefaultImpulseMultiplier;
        }

        public static LayerInteraction CreateKinematicInteraction(int kinematicLayer, int dynamicLayer) {
            return new LayerInteraction(kinematicLayer, dynamicLayer, 0);
        }
        
        public void ResetToDefault() {
            impulseMultiplier = DefaultImpulseMultiplier;
        }

        public static LayerInteraction CreateKinematicInteraction(PhysicsLayer kinematicLayer, PhysicsLayer dynamicLayer) {
            return CreateKinematicInteraction(kinematicLayer.Number, dynamicLayer.Number);
        }
    }

    [Serializable]
    public struct OneWayLayerInteraction {
        public const float DefaultImpulseMultiplier = 1f;
        
        public int receiver;
        public float impulseMultiplier;

        public OneWayLayerInteraction(int receiver) {
            this.receiver = receiver;

            impulseMultiplier = DefaultImpulseMultiplier;
        }
        
        public void ResetToDefault() {
            impulseMultiplier = DefaultImpulseMultiplier;
        }
    }
}