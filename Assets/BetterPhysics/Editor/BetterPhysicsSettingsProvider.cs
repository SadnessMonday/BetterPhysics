using SadnessMonday.BetterPhysics.Layers;
using UnityEditor;
using UnityEngine;

// This package changed in 2019.1
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#elif UNITY_2018_4_OR_NEWER
#endif

namespace SadnessMonday.BetterPhysics.Editor {

    // Create SmartNSSettingsProvider by deriving from SettingsProvider:
    [InitializeOnLoad]
    public class BetterPhysicsSettingsProvider : SettingsProvider
    {
        private SerializedObject _settings;
        private Vector2 _interactionsScrollPos;
        
        class Styles {
            public static readonly GUIContent LayerNamesStorage = new("Layers", "Set up your layer names here");
            public static readonly GUIContent InteractionsStorage = new("Interactions", "Set up your custom interactions here");
        }

        public BetterPhysicsSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope) { }

        public static bool IsSettingsAvailable() {
            return BetterPhysicsSettings.Instance != null;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // This function is called when the user clicks on the SmartNSSettings element in the Settings window.
            _settings = new SerializedObject(BetterPhysicsSettings.Instance);
        }

        public override void OnDeactivate()
        {
            AssetDatabase.SaveAssets();
        }

        public override void OnGUI(string searchContext)
        {
            _settings.Update();

            EditorGUILayout.LabelField($"Version {BetterPhysics.BetterPhysicsVersion}");

            // Preferences GUI
            EditorGUILayout.HelpBox("These layers are used for custom interactions between BetterRigidbodies.", MessageType.None);
            EditorGUIUtility.labelWidth = 245.0f;

            EditorGUILayout.PropertyField(_settings.FindProperty("layerNamesStorage"), Styles.LayerNamesStorage);
            
            // _interactionsScrollPos = EditorGUILayout.BeginScrollView(_interactionsScrollPos, GUILayout.Height(120));
            // EditorGUILayout.PropertyField(_settings.FindProperty("interactionsStorage"), Styles.InteractionsStorage);
            // EditorGUILayout.EndScrollView();
                 
            LayerInteractionMatrixGUI.Draw(new GUIContent("Layer Interactions"), BetterPhysics.GetLayerInteraction, BetterPhysics.SetLayerInteraction);
            if (GUILayout.Button("Reset Better Physics Settings")) {
                ((BetterPhysicsSettings)_settings.targetObject).Reset();
                _settings.Update();
            }
            
            _settings.ApplyModifiedProperties();
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateBetterPhysicsSettingsProvider()
        {
            if (!IsSettingsAvailable()) {
                return null;
            }
            
            //Debug.Log("Settings Available");
            var provider = new BetterPhysicsSettingsProvider("Project/Better Physics") {
                // Automatically extract all keywords from the Styles.
                keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
            };

            return provider;
        }
    }
}