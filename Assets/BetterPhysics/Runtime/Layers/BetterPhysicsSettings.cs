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
        private const int DefaultUserLayerCount = 3;
        private const string DefaultUserLayerNameFormat = "User Layer {0}";
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
            settings.Reset();
            
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


        [SerializeField] private List<string> layerNamesStorage = new();
        private Dictionary<string, int> _layerNamesLookup = new();

        [SerializeField] private List<InteractionConfiguration> interactionsStorage = new();
        private Dictionary<Vector2Int, InteractionConfiguration> _interactionsLookup = new();

        public IReadOnlyList<string> AllLayerNames => layerNamesStorage;

        private void Awake() {
            if (_instance == null) _instance = this;
            Init();
        }

        public void Reset() {
            Debug.Log(nameof(Reset));
            if (_instance == null) _instance = this;
            ResetAllLayerNames();
            ResetAllLayerInteractions();
        }

        void ResetAllLayerNames() {
            layerNamesStorage.Clear();
            _layerNamesLookup.Clear();
            PopulateDefaultLayerNames();
        }
        
        void PopulateDefaultLayerNames() {
            // Create 
            layerNamesStorage.AddRange(InteractionLayer.GetBuiltinLayerNames());
            for (int i = 0; i < DefaultUserLayerCount; i++) {
                layerNamesStorage.Add(string.Format(DefaultUserLayerNameFormat, i));
            }
            Debug.Log($"Populated {layerNamesStorage.Count} layers");
        }
        
        public void ResetAllLayerInteractions() {
            Debug.Log(nameof(ResetAllLayerInteractions));
            interactionsStorage.Clear();
            _interactionsLookup.Clear();
            PopulateDefaultInteractions();
            
            Init();
        }

        void PopulateDefaultInteractions() {
            Debug.Log(nameof(PopulateDefaultInteractions));

            for (int i = 0; i < layerNamesStorage.Count; i++) {
                interactionsStorage.Add(InteractionConfiguration.CreateKinematicInteraction(new InteractionLayer(i), InteractionLayer.FeatherLayer));
                interactionsStorage.Add(InteractionConfiguration.CreateKinematicInteraction( InteractionLayer.UnstoppableLayer, new InteractionLayer(i)));
            }
        }

        private void InitLayerNames() {
            _layerNamesLookup.Clear();
            for (int i = 0; i < layerNamesStorage.Count; i++) {
                string layerName = layerNamesStorage[i];
                _layerNamesLookup[layerName] = i;
            }
        }

        private void InitLayerInteractions() {
            foreach (var interaction in interactionsStorage) {
                Vector2Int coord = interaction.Key();
                coord.Normalize();
                // if (!LayerIsDefined(coord.x) || !LayerIsDefined(coord.y)) {
                //     Debug.LogWarning($"Found interaction involving an undefined layer: {interaction}");
                //     continue;
                // }
                
                if (_interactionsLookup.ContainsKey(coord)) {
                    Debug.LogWarning($"Two or more BetterPhysics interactions defined between layers {coord.x} and {coord.y}");
                }
                
                _interactionsLookup[coord] = interaction;
            }
        }

        public void Init() {
            InitLayerNames();
            InitLayerInteractions();
        }

        public bool TryGetLayerFromName(string layerName, out InteractionLayer output) {
            if (_layerNamesLookup.TryGetValue(layerName, out int index)) {
                output = new InteractionLayer(index);
                return true;
            }

            output = InteractionLayer.InvalidLayer;
            return false;
        }

        public InteractionLayer LayerFromName(string layerName) {
            if (!_layerNamesLookup.TryGetValue(layerName, out int result)) {
                throw new ArgumentException($"Layer '{layerName}' has not been defined in BetterPhysics settings");
            }
            
            return new InteractionLayer(result);
        }

        public string LayerIndexToName(int layerIndex) {
            if (LayerIsDefined(layerIndex)) return layerNamesStorage[layerIndex];

            throw new ArgumentException($"Layer index {layerIndex} was not defined!");
        }

        public bool LayerIsDefined(string layer) {
            return _layerNamesLookup.ContainsKey(layer);
        }

        public bool LayerIsDefined(int layer) {
            return layer >= 0 && layer < layerNamesStorage.Count;
        }

        public InteractionConfiguration GetInteractionOrDefault(int actor, int receiver) {
            return GetInteractionOrDefault(new(actor, receiver));
        }

        public InteractionConfiguration GetInteractionOrDefault(Vector2Int key) {
            key.Normalize();
            if (TryGetLayerInteraction(key, out InteractionConfiguration configuration)) {
                return configuration;
            }

            return new InteractionConfiguration(new(key.x), new(key.y), InteractionType.Default);
        }
        
        public bool TryGetLayerInteraction(InteractionLayer actor, InteractionLayer receiver, out InteractionConfiguration interactionConfiguration) {
            return TryGetLayerInteraction(actor.KeyWith(receiver), out interactionConfiguration);
        }

        internal bool TryGetLayerInteraction(int actor, int receiver, out InteractionConfiguration interactionConfiguration) {
            return TryGetLayerInteraction(new Vector2Int(actor, receiver), out interactionConfiguration);
        }

        internal bool TryGetLayerInteraction(Vector2Int key, out InteractionConfiguration interactionConfiguration) {
            return _interactionsLookup.TryGetValue(key, out interactionConfiguration);
        }
        
        public void SetLayerInteraction(Vector2Int key, InteractionType interactionType) {
            Debug.Log($"Attempting to set interaction {key.x}/{key.y} to {interactionType}");
            if (InteractionLayer.IncludesReservedLayers(key)) {
                throw new BetterPhysicsException($"Cannot modify interaction with reserved layers");
            }
            
            key.Normalize();
            if (interactionType == InteractionType.Default) {
                _interactionsLookup.Remove(key);
                return;
            }
            
            InteractionConfiguration interactionConfiguration = new InteractionConfiguration(InteractionLayer.FromIndex(key.x), InteractionLayer.FromIndex(key.y));
            _interactionsLookup[key] = interactionConfiguration;
        }
        
        public void SetLayerInteraction(InteractionLayer actor, InteractionLayer receiver, InteractionType interactionType) {
            Vector2Int key = actor.KeyWith(receiver);
            SetLayerInteraction(key, interactionType);
        }

        public void SetLayerInteraction(int actor, int receiver, InteractionType interactionType) {
            SetLayerInteraction(new InteractionLayer(actor), new InteractionLayer(receiver), interactionType);
        }

        /**
         * Resets the interaction between actor and receiver to default
         */
        public bool ResetLayerInteraction(InteractionLayer actor, InteractionLayer receiver) {
            return _interactionsLookup.Remove(actor.KeyWith(receiver));
        }

        public InteractionLayer AddLayer(string name) {
            if (TryGetLayerFromName(name, out InteractionLayer layer)) {
                return layer;
            }

            var index = layerNamesStorage.Count;
            _layerNamesLookup[name] = index;
            layerNamesStorage.Add(name);

            return new InteractionLayer(index);
        }
    }
}