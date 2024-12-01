using System;
using System.Collections;
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
    public class BetterPhysicsSettings : ScriptableObject, ISerializationCallbackReceiver {
        private const int DefaultUserLayerCount = 3;
        private const string DefaultUserLayerNameFormat = "User Layer {0}";
        private const string DefaultSettingsAssetName = "BetterPhysicsSettings";
        private static BetterPhysicsSettings _instance;
        public int DefinedLayerCount => _layerNamesLookup.Count;

        public static BetterPhysicsSettings Instance {
            get {
                if (ReferenceEquals(_instance, null)) {
#if UNITY_EDITOR
                    _instance = GetLayerSettingsAssetInEditor();
#else    
                    _instance = Resources.Load<BetterPhysicsSettings>(DefaultSettingsAssetName);
#endif

                    if (ReferenceEquals(_instance, null)) {
                        Debug.LogWarning("Could not find a BetterPhysics settings asset. Falling back to creating one at runtime");
                        _instance = CreateInstance<BetterPhysicsSettings>();
                        _instance.Reset();
                    }
                }
                
#if UNITY_EDITOR
                if (UnityEditor.AssetDatabase.GetAssetPath(_instance) == null) {
                    Directory.CreateDirectory(DefaultSettingsAssetPath);
                    AssetDatabase.CreateAsset(_instance, DefaultSettingsAssetFullPath);
                    AssetDatabase.SaveAssets();
                }
#endif

                return _instance;
            }
        }

#if UNITY_EDITOR
        private const string DefaultSettingsAssetPath = "Assets/BetterPhysics/Runtime/Settings/Resources";
        private const string DefaultSettingsAssetFileName = DefaultSettingsAssetName + ".asset";

        private static string DefaultSettingsAssetFullPath =>
            Path.Combine(DefaultSettingsAssetPath, DefaultSettingsAssetFileName);

        private static BetterPhysicsSettings GetLayerSettingsAssetInEditor() {
            string path = GetSettingsFilePathInEditor();
            BetterPhysicsSettings instance = AssetDatabase.LoadAssetAtPath<BetterPhysicsSettings>(path);
            if (instance != null) return instance;

            Debug.Log($"Creating new BetterPhysicsSettings instance");
            path = DefaultSettingsAssetFullPath;
            var settings = CreateInstance<BetterPhysicsSettings>();
            settings.Reset();

            Directory.CreateDirectory(DefaultSettingsAssetPath);
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
        public IReadOnlyList<string> AllLayerNamesNumbered => new NumberedLayerList(layerNamesStorage);
        
        private readonly struct NumberedLayerList : IReadOnlyList<string> {
            private readonly List<string> backingList;
            
            public NumberedLayerList(List<string> backingList) {
                this.backingList = backingList;
                this.Count = backingList.Count;
            }
            
            public IEnumerator<string> GetEnumerator() {
                for (int i = 0; i < backingList.Count; i++) {
                    yield return $"{i}: {backingList[i]}";
                }
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            public int Count { get; }

            public string this[int index] {
                get {
                    if (index < 0) throw new IndexOutOfRangeException();

                    if (index >= backingList.Count) return $"{index} : <undefined layer>";
                    
                    return $"{index} : {backingList[index]}";
                }
            } 
        }

        private void Awake() {
            if (_instance == null) _instance = this;
            Init();
        }

        public void Reset() {
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

            for (int i = 0; i < layerNamesStorage.Count; i++) {
                var name = layerNamesStorage[i];
                _layerNamesLookup[name] = i;
            }
        }
        
        public void ResetAllLayerInteractions() {
            // Debug.Log(nameof(ResetAllLayerInteractions));
            interactionsStorage.Clear();
            _interactionsLookup.Clear();
            
            Init();
        }

        public void Init() {
            // These shouldn't be necessary anymore with the serialization stuff.
            // InitLayerNames();
            // InitLayerInteractions();
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
            if (TryGetLayerInteraction(key, out InteractionConfiguration configuration)) {
                return configuration;
            }

            return new InteractionConfiguration(key, InteractionType.Default);
        }
        
        public bool TryGetLayerInteraction(InteractionLayer actor, InteractionLayer receiver, out InteractionConfiguration interactionConfiguration) {
            return TryGetLayerInteraction(actor.KeyWith(receiver), out interactionConfiguration);
        }

        internal bool TryGetLayerInteraction(int actor, int receiver, out InteractionConfiguration interactionConfiguration) {
            return TryGetLayerInteraction(new Vector2Int(actor, receiver), out interactionConfiguration);
        }

        internal bool TryGetLayerInteraction(Vector2Int key, out InteractionConfiguration interactionConfiguration) {
            // Check default layers
            if (key.x == InteractionLayer.UnstoppableLayerIndex && LayerIsDefined(key.y)) {
                interactionConfiguration = new InteractionConfiguration(key, InteractionType.Kinematic);
                return true;
            }
            if (key.x == InteractionLayer.FeatherLayerIndex && LayerIsDefined(key.y)) {
                interactionConfiguration = new InteractionConfiguration(key, InteractionType.Feather);
                return true;
            }
            if (key.y == InteractionLayer.UnstoppableLayerIndex && LayerIsDefined(key.x)) {
                interactionConfiguration = new InteractionConfiguration(key, InteractionType.Feather);
                return true;
            }
            if (key.y == InteractionLayer.FeatherLayerIndex && LayerIsDefined(key.x)) {
                interactionConfiguration = new InteractionConfiguration(key, InteractionType.Kinematic);
                return true;
            }
            
            bool flipped = key.NormalizedCopy(out Vector2Int normalizedKey);
            if (_interactionsLookup.TryGetValue(normalizedKey, out interactionConfiguration)) {
                if (flipped) {
                    interactionConfiguration = interactionConfiguration.Inverse();
                }
                
                return true;
            }

            return false;
        }
        
        #if UNITY_EDITOR
        public void UpdateLayerInteractionMatrix(Vector2Int key, InteractionType interactionType) {
            // Debug.Log($"Updating interaction matrix on {this.GetInstanceID()}");
            key.Normalize(ref interactionType);
            if (DefinedLayerCount <= key.x || DefinedLayerCount <= key.y) {
                throw new BetterPhysicsException("Cannot set an interaction with an undefined layer");
            }

            if (InteractionLayer.IncludesReservedLayers(key)) {
                throw new BetterPhysicsException($"Cannot modify interaction with reserved layers");
            }

            bool foundIt = false;
            var newInteractionConfig = new InteractionConfiguration(key, interactionType);
            // find existing interaction if any
            for (int i = 0; i < interactionsStorage.Count; i++) {
                var interaction = interactionsStorage[i];
                if (interaction.Key().Equals(key)) {
                    // found it
                    if (interactionType == InteractionType.Default) {
                        // remove it
                        interactionsStorage.RemoveAt(i);
                    }
                    else {
                        interactionsStorage[i] = newInteractionConfig;
                    }

                    foundIt = true;
                    break;
                }
            }

            if (!foundIt && interactionType != InteractionType.Default) {
                // Create a new one
                interactionsStorage.Add(newInteractionConfig);
            }
            
            SetLayerInteraction(key, interactionType);
        }
        #endif

        public void SetLayerInteraction(Vector2Int key, InteractionType interactionType) {
            // Debug.Log($"Attempting to set interaction {key.x}/{key.y} to {interactionType}");
            if (InteractionLayer.IncludesReservedLayers(key)) {
                throw new BetterPhysicsException($"Cannot modify interaction with reserved layers");
            }
            
            // We only store normalized interactions.
            bool flipped = key.Normalize();
            if (flipped) {
                interactionType = interactionType.Inverse();
            }
            
            if (interactionType == InteractionType.Default) {
                _interactionsLookup.Remove(key);
                return;
            }
            
            InteractionConfiguration interactionConfiguration = new InteractionConfiguration(key, interactionType);
            _interactionsLookup[key] = interactionConfiguration;
        }
        
        public void SetLayerInteraction(InteractionLayer actor, InteractionLayer receiver, InteractionType interactionType) {
            Vector2Int key = actor.KeyWith(receiver);
            SetLayerInteraction(key, interactionType);
        }

        public void SetLayerInteraction(int actor, int receiver, InteractionType interactionType) {
            SetLayerInteraction(new InteractionLayer(actor), new InteractionLayer(receiver), interactionType);
        }
        

        public void SetLayerInteraction(InteractionConfiguration configuration) {
            SetLayerInteraction(configuration.UnsafeKey(), configuration.InteractionType);
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

            var index = DefinedLayerCount;
            _layerNamesLookup[name] = index;
            layerNamesStorage.Add(name);

            return new InteractionLayer(index);
        }

        public void OnBeforeSerialize() {
            interactionsStorage.Clear();
            layerNamesStorage.Clear();
            foreach (var pair in _interactionsLookup) {
                interactionsStorage.Add(pair.Value);
            }

            for (int i = 0; i < _layerNamesLookup.Count; i++) {
                layerNamesStorage.Add(null);
            }
            foreach (var pair in _layerNamesLookup) {
                layerNamesStorage[pair.Value] = pair.Key;
            }
        }

        public void OnAfterDeserialize() {
            _interactionsLookup.Clear();
            foreach (var interaction in interactionsStorage) {
                _interactionsLookup[interaction.UnsafeKey()] = interaction;
            }
            
            _layerNamesLookup.Clear();
            for (int i = 0; i < layerNamesStorage.Count; i++) {
                _layerNamesLookup[layerNamesStorage[i]] = i;
            }
        }
    }
}