using System;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Layers {
    [Serializable]
    public struct InteractionConfiguration {
        [SerializeField] private int _actorLayer;
        [SerializeField] private int _receiverLayer;

        public InteractionLayer Actor {
            get => InteractionLayer.FromIndex(_actorLayer);
            set => _actorLayer = value.Index;
        }

        public InteractionLayer Receiver {
            get => InteractionLayer.FromIndex(_receiverLayer);
            set => _receiverLayer = value.Index;
        }

        [field: SerializeField] public InteractionType InteractionType { get; private set; }

        public InteractionConfiguration(InteractionLayer actor, InteractionLayer receiver,
            InteractionType interactionType = InteractionType.Default) {
            _actorLayer = actor.Index;
            _receiverLayer = receiver.Index;

            InteractionType = interactionType;

            Normalize();
        }

        public InteractionConfiguration(Vector2Int key, InteractionType interactionType = InteractionType.Default) {
            _actorLayer = key.x;
            _receiverLayer = key.y;
            InteractionType = interactionType;
            
            Normalize();
        }

        public static InteractionConfiguration CreateKinematicInteraction(InteractionLayer kinematicLayer,
            InteractionLayer dynamicLayer) {
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