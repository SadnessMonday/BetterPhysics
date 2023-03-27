using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SadnessMonday.BetterPhysics.Layers {
    /**
     * Represents the custom settings page for physics layers
     */
    public class BetterPhysicsSettings : ScriptableObject {
        private const string DefaultSettingsAssetName = "BetterPhysicsSettings";
        private static BetterPhysicsSettings _instance;
        public int DefinedLayerCount => layerNamesStorage.Count;

        public static BetterPhysicsSettings Instance {
            get {
                if (ReferenceEquals(_instance, null)) {
#if UNITY_EDITOR
                    _instance = GetLayerSettingsAssetInEditor();
#else    
                    _instance = Resources.Load(DefaultLayerAssetName);
#endif

                    if (ReferenceEquals(_instance, null)) {
                        Debug.LogWarning("Could not find a BetterPhysics settings asset. Falling back to creating one at runtime");
                        _instance = CreateInstance<BetterPhysicsSettings>();
                    }
                }

                return _instance;
            }
        }

#if UNITY_EDITOR
        
        private const string DefaultSettingsAssetPath = "Assets/BetterPhysics/Resources";
        private const string DefaultSettingsAssetFileName = DefaultSettingsAssetName + ".asset";

        private static BetterPhysicsSettings GetLayerSettingsAssetInEditor() {
            string path = GetSettingsFilePathInEditor();
            BetterPhysicsSettings instance = AssetDatabase.LoadAssetAtPath<BetterPhysicsSettings>(path);
            if (instance != null) return instance;

            path = Path.Combine(DefaultSettingsAssetPath, DefaultSettingsAssetFileName);
            var settings = CreateInstance<BetterPhysicsSettings>();
            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();

            return settings;
        }
        
        private static string GetSettingsFilePathInEditor() {
            // Although there is a default location for thr Settings, we want to be able to find it even if the 
            // player has moved them around. This will locate the settings even if they're not in the default location.
            var assetGUIDs = AssetDatabase.FindAssets($"t:{nameof(BetterPhysicsSettings)}");

            if (assetGUIDs.Length > 1) {
                var paths = string.Join(", ", assetGUIDs.Select(AssetDatabase.GUIDToAssetPath));
                Debug.LogWarning(
                    $"Multiple {DefaultSettingsAssetFileName} files exist in this project. This may lead to confusion, as any of the settings files may be chosen arbitrarily. You should remove all but one of the following so that you only have one {DefaultSettingsAssetFileName} file: {paths}");
            }

            if (assetGUIDs.Length > 0) {
                return AssetDatabase.GUIDToAssetPath(assetGUIDs.First());
            }

            return null;
        }
#endif

        [RuntimeInitializeOnLoadMethod]
        private static void WhenGameStarts() {
            var instance = Instance;
            instance.Init();
        }


        [SerializeField] internal List<string> layerNamesStorage = new();
        private Dictionary<string, int> _layerNamesLookup = new();

        [SerializeField] private List<LayerInteraction> interactionsStorage = new();
        private Dictionary<Vector2Int, LayerInteraction> _interactionsLookup = new();

        public IReadOnlyList<string> AllLayerNames => layerNamesStorage;

        private void Awake() {
            Reset();
        }

        public void Reset() {
            layerNamesStorage.Clear();
            interactionsStorage.Clear();
            
            // Create 
            layerNamesStorage.AddRange(PhysicsLayer.GetBuiltinLayers().Select(l => l.Name));
            
            // TODO actually we should be able to do something like "ANY LAYER" as the other side.
            interactionsStorage.Add(LayerInteraction.CreateKinematicInteraction(PhysicsLayer.DefaultLayer, PhysicsLayer.FeatherLayer));
            interactionsStorage.Add(LayerInteraction.CreateKinematicInteraction(PhysicsLayer.UnstoppableLayer, PhysicsLayer.FeatherLayer));
            interactionsStorage.Add(LayerInteraction.CreateKinematicInteraction(PhysicsLayer.FeatherLayer, PhysicsLayer.FeatherLayer));
            interactionsStorage.Add(LayerInteraction.CreateKinematicInteraction(PhysicsLayer.UnstoppableLayer, PhysicsLayer.DefaultLayer));
        }

        public void Init() {
            Debug.Log("Initializing settings");
            InitLayerNames();
            InitLayerInteractions();
        }

        private void InitLayerNames() {
            _layerNamesLookup.Clear();
            for (int i = 0; i < layerNamesStorage.Count; i++) {
                string layerName = layerNamesStorage[i];
                _layerNamesLookup[layerName] = i;
            }
        }

        private void InitLayerInteractions() {
            ResetAllLayerInteractions();
            foreach (var interaction in interactionsStorage) {
                Vector2Int coord = new(interaction.actor, interaction.receiver);
                if (!LayerIsDefined(coord.x) || !LayerIsDefined(coord.y)) {
                    Debug.LogWarning($"Found interaction involving an undefined layer: {interaction}");
                    continue;
                }

                if (_interactionsLookup.ContainsKey(coord)) {
                    Debug.LogWarning($"Two or more BetterPhysics interactions defined between layers {coord.x} and {coord.y}");
                }
                
                _interactionsLookup[coord] = interaction;
            }
        }

        public int LayerNameToIndex(string layerName) {
            if (!_layerNamesLookup.TryGetValue(layerName, out int result)) {
                throw new ArgumentException($"Layer '{layerName}' has not been defined in BetterPhysics settings");
            }
            
            return result;
        }

        public string LayerIndexToName(int layerIndex) {
            if (LayerIsDefined(layerIndex)) return layerNamesStorage[layerIndex];

            throw new ArgumentException($"Layer index {layerIndex} was not defined!");
        }

        public bool LayerIsDefined(string layer) {
            return _layerNamesLookup.ContainsKey(layer);
        }

        public bool LayerIsDefined(long layer) {
            return layer >= 0 && layer < layerNamesStorage.Count;
        }
        
        public bool TryGetLayerInteraction(int actor, int receiver, out LayerInteraction interaction) {
            return _interactionsLookup.TryGetValue(new Vector2Int(actor, receiver), out interaction);
        }

        public void SetLayerIteraction(int actor, int receiver, LayerInteraction.InteractionType interactionType) {
            Vector2Int key = new Vector2Int(actor, receiver);
            if (interactionType == LayerInteraction.InteractionType.Default) {
                _interactionsLookup.Remove(key);
                return;
            }
            
            LayerInteraction interaction = new LayerInteraction(actor, receiver);
            interaction.interactionType = interactionType;
            _interactionsLookup[key] = interaction;
        }

        /**
         * Resets the interaction between actor and receiver to default
         */
        public bool ResetLayerInteraction(int actor, int receiver) {
            return _interactionsLookup.Remove(new Vector2Int(actor, receiver));
        }

        public void ResetAllLayerInteractions() {
            _interactionsLookup.Clear();
        }
    }
}