using SadnessMonday.BetterPhysics.Layers;
using UnityEditor;
using UnityEngine;

// This package changed in 2019.1
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#elif UNITY_2018_4_OR_NEWER
using UnityEngine.Experimental.UIElements;
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

            // public static GUIContent ScriptRoot = new GUIContent("Script Root", SmartNSSettings.SCRIPT_ROOT_TOOLTIP);
            // public static GUIContent NamespacePrefix = new GUIContent("Namespace Prefix", SmartNSSettings.NAMESPACE_PREFIX_TOOLTIP);
            // public static GUIContent UniversalNamespace = new GUIContent("Universal Namespace", SmartNSSettings.UNIVERSAL_NAMESPACE_TOOLTIP);
            // public static GUIContent IndentUsingSpaces = new GUIContent("Indent using Spaces", SmartNSSettings.INDENT_USING_SPACES_TOOLTIP);
            // public static GUIContent NumberOfSpaces = new GUIContent("Number of Spaces", SmartNSSettings.NUMBER_OF_SPACES_TOOLTIP);
            // //public static GUIContent DefaultScriptCreationDirectory = new GUIContent("Default Script Creation Directory", "(Experimental) If you specify a path here, any scripts created directly within 'Assets' will instead be created in the folder you specify. (No need to prefix this with 'Assets'.)");
            // public static GUIContent UpdateNamespacesWhenMovingScripts = new GUIContent("Update Namespaces When Moving Scripts", SmartNSSettings.UPDATE_NAMESPACES_WHEN_MOVING_SCRIPTS_TOOLTIP);
            // public static GUIContent DirectoryIgnoreList = new GUIContent("Directory Deny List (One directory per line)", SmartNSSettings.DIRECTORY_IGNORE_LIST_TOOLTIP);
            // public static GUIContent EnableDebugLogging = new GUIContent("Enable Debug Logging", SmartNSSettings.ENABLE_DEBUG_LOGGING_TOOLTIP);
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
            
            _interactionsScrollPos = EditorGUILayout.BeginScrollView(_interactionsScrollPos, GUILayout.Height(120));
            EditorGUILayout.PropertyField(_settings.FindProperty("interactionsStorage"), Styles.InteractionsStorage);
            EditorGUILayout.EndScrollView();

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