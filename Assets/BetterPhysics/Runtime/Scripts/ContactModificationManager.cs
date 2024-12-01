using System.Collections.Generic;
using SadnessMonday.BetterPhysics.Layers;
using Unity.Collections;
using UnityEngine;

namespace SadnessMonday.BetterPhysics {
    
    // This needs to run before BetterRigidbody
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("")] // Disallow adding from Add Component Menu
    internal class ContactModificationManager : MonoBehaviour {
        private static ContactModificationManager _instance;
        public static ContactModificationManager Instance {
            get {
                if (_instance != null) return _instance;

                GameObject go = new("BetterPhysics");
                _instance = go.AddComponent<ContactModificationManager>();
                DontDestroyOnLoad(go);

                return _instance;
            }
        }

        private Dictionary<int, IDictionary<int, OneWayLayerInteraction>> _perRigidbodyData;
        
        // Tracks which BetterRigidbody is in which layer
        private Dictionary<int, int> _rigidbodyLayerMapping;
        
        private BetterPhysicsSettings _settings;

        private void Awake() {
            _rigidbodyLayerMapping = new();
            _perRigidbodyData = new();
            _settings = BetterPhysicsSettings.Instance;
        }

        private void OnEnable() {
            Physics.ContactModifyEvent += PhysicsOnContactModifyEvent;
        }

        private void OnDisable() {
            Physics.ContactModifyEvent -= PhysicsOnContactModifyEvent;
        }
        
        private void PhysicsOnContactModifyEvent(PhysicsScene scene, NativeArray<ModifiableContactPair> contactPairs) {
            for (int i = 0; i < contactPairs.Length; i++) {
                var pair = contactPairs[i];
                
                int bodyAId = pair.bodyInstanceID;
                int bodyBId = pair.otherBodyInstanceID;
                
                if (!_rigidbodyLayerMapping.TryGetValue(bodyAId, out int layerA)) {
                    // Debug.Log($"Body A {bodyAId} is not a registered BRB");
                    // Body A is not a registered BRB, we can ignore this contact.
                    continue;
                }
                
                if (!_rigidbodyLayerMapping.TryGetValue(bodyBId, out int layerB)) {
                    // Debug.Log($"Body B {bodyBId} is not a registered BRB");
                    // Body B is not a registered BRB, we can ignore this contact.
                    continue;
                }
                // Debug.Log($"Found body {bodyAId} on layer {layerA} and {bodyBId} on {layerB}");
                
                int aToBMultiplier = 1;
                int bToAMultiplier = 1;
                
                if (_perRigidbodyData.TryGetValue(bodyAId, out var bodyAInteractions)
                    && bodyAInteractions.TryGetValue(layerB, out OneWayLayerInteraction owInteractionA)) {
                    if (owInteractionA.interactionType == InteractionType.Kinematic) {
                        aToBMultiplier = 0;
                    }
                    else if (owInteractionA.interactionType == InteractionType.Feather) {
                        bToAMultiplier = 0;
                    }
                }                
                else if (_settings.TryGetLayerInteraction(layerA, layerB, out InteractionConfiguration aToBInteraction)) {
                    if (aToBInteraction.InteractionType == InteractionType.Kinematic) {
                        aToBMultiplier = 0;
                    }
                    else if (aToBInteraction.InteractionType == InteractionType.Feather) {
                        bToAMultiplier = 0;
                    }
                }
                // else {
                //     Debug.Log($"Found nothing for {layerA} to {layerB}");
                // }

                if (_perRigidbodyData.TryGetValue(bodyBId, out var bodyBInteractions)
                    && bodyBInteractions.TryGetValue(layerB, out OneWayLayerInteraction owInteractionB)) {
                    if (owInteractionB.interactionType == InteractionType.Kinematic) {
                        bToAMultiplier = 0;
                    }
                    else if (owInteractionB.interactionType == InteractionType.Feather) {
                        aToBMultiplier = 0;
                    }
                }                
                else if (_settings.TryGetLayerInteraction(layerB, layerA, out InteractionConfiguration bToAInteraction)) {
                    if (bToAInteraction.InteractionType == InteractionType.Kinematic) {
                        bToAMultiplier = 0;
                    }
                    else if (bToAInteraction.InteractionType == InteractionType.Feather) {
                        aToBMultiplier = 0;
                    }
                }
                // else {
                //     Debug.Log($"Found nothing for {layerB} to {layerA}");
                // }

                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (aToBMultiplier == 1 && bToAMultiplier == 1) {
                    // Debug.Log("Found nothing for either layer");
                    continue;
                }
                // ReSharper restore CompareOfFloatsByEqualityOperator

                // Debug.Log($"Instance {bodyAId} of layer {layerA} collided with {bodyBId} of {layerB}." +
                //           $" {layerA} to {layerB} modifier was {aToBMultiplier} and " +
                //           $"{layerB} to {layerA} was {bToAMultiplier}");
                
                var massProperties = pair.massProperties;
                massProperties.inverseMassScale = aToBMultiplier;
                massProperties.inverseInertiaScale = aToBMultiplier;
                massProperties.otherInverseMassScale = bToAMultiplier;
                massProperties.otherInverseInertiaScale = bToAMultiplier;
                pair.massProperties = massProperties;

                contactPairs[i] = pair;
            }
        }

        public void Register(BetterRigidbody body) {
            var rbInstanceId = body.GetRigidbodyInstanceID();
            _rigidbodyLayerMapping[rbInstanceId] = body.PhysicsLayer;
            _perRigidbodyData[rbInstanceId] = body.SerializedInteractions;
            
            // Debug.Log($"Registering {rbInstanceId} to layer {body.PhysicsLayer}");
        }

        public void UnRegister(BetterRigidbody body) {
            var rbInstanceId = body.GetRigidbodyInstanceID();
            _perRigidbodyData.Remove(rbInstanceId);
            _rigidbodyLayerMapping.Remove(rbInstanceId);
        }

        public void ResetCustomInteractions(BetterRigidbody body) {
            var rbInstanceId = body.GetRigidbodyInstanceID();
            if (_perRigidbodyData.TryGetValue(rbInstanceId, out var data)) {
                data.Clear();
            }
        }

        public void SetCustomInteraction(BetterRigidbody body, OneWayLayerInteraction interaction) {
            var rbInstanceId = body.GetRigidbodyInstanceID();
            if (_perRigidbodyData.TryGetValue(rbInstanceId, out var data)) {
                data[interaction.receiver] = interaction;
            }
        }

        public bool RemoveCustomInteraction(BetterRigidbody body, int receiverLayer) {
            var rbInstanceId = body.GetRigidbodyInstanceID();
            if (_perRigidbodyData.TryGetValue(rbInstanceId, out var data)) {
                return data.Remove(receiverLayer);
            }

            return false;
        }

        public bool TryGetCustomInteraction(BetterRigidbody body, int receiverLayer, out OneWayLayerInteraction interaction) {
            var rbInstanceId = body.GetRigidbodyInstanceID();
            if (_perRigidbodyData.TryGetValue(rbInstanceId, out var data)) {
                return data.TryGetValue(receiverLayer, out interaction);
            }

            interaction = default;
            return false;
        }

        public void UpdateBodyLayer(BetterRigidbody body) {
            var rbInstanceId = body.GetRigidbodyInstanceID();
            _rigidbodyLayerMapping.Remove(rbInstanceId);
            _rigidbodyLayerMapping[rbInstanceId] = body.PhysicsLayer;
        }
    }
}