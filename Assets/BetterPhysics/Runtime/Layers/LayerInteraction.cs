using System;
using UnityEngine.Serialization;

namespace SadnessMonday.BetterPhysics.Layers {
    [Serializable]
    public struct LayerInteraction {
        public const float DefaultImpulseMultiplier = 1f;
        
        public int actor;
        public int receiver;
        public float impulseMultiplier;

        public LayerInteraction(int actor, int receiver) {
            this.actor = actor;
            this.receiver = receiver;

            impulseMultiplier = DefaultImpulseMultiplier;
        }
        
        public void ResetToDefault() {
            impulseMultiplier = DefaultImpulseMultiplier;
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