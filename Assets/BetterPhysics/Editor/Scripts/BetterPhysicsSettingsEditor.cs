using SadnessMonday.BetterPhysics.Layers;
using UnityEditor;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Editor {
    [CustomEditor(typeof(BetterPhysicsSettings))]
    public class BetterPhysicsSettingsEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            EditorGUILayout.LabelField("Please do not modify this asset directly.");
            if (GUILayout.Button("Open the BetterPhysics Settings Window")) {
                SettingsService.OpenProjectSettings("Project/Better Physics");
            }
        }
    }
}