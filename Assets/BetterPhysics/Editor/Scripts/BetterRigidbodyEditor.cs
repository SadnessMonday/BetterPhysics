using System.Linq;
using SadnessMonday.BetterPhysics.Layers;
using UnityEditor;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Editor {

    [CustomEditor(typeof(BetterRigidbody))]
    public class BetterRigidbodyEditor : UnityEditor.Editor
    {
        private static GUIContent LimitContent = new GUIContent("Limit:", "Negative means unlimited.");
        private GUIStyle rtStyle;
        private const string SpeedLimitsLabelPrefix = "Speed Limits";
        private const string SoftLimitSpecifier = "<b>Soft</b>";
        private const string HardLimitSpecifier = "<b>Hard</b>";
        private const string BothLimits = SpeedLimitsLabelPrefix + " [" + SoftLimitSpecifier + ", " + HardLimitSpecifier + "]";
        private const string SoftLimitOnly = SpeedLimitsLabelPrefix + " [" + SoftLimitSpecifier + "]";
        private const string HardLimitOnly = SpeedLimitsLabelPrefix + " [" + HardLimitSpecifier + "]";
        
        private bool showAdvanced;
        private bool showBaseRigidbodySettings;

        private SerializedProperty layerField;

        private class Styles
        {
            public static GUIContent includeLayers = EditorGUIUtility.TrTextContent("Include Layers", "Layers to include when producing collisions");
            public static GUIContent excludeLayers = EditorGUIUtility.TrTextContent("Exclude Layers", "Layers to exclude when producing collisions");
        }

        private void OnEnable() {
            layerField = serializedObject.FindProperty("physicsLayer");
        }

        public override void OnInspectorGUI()
        {
            // annoyingly this can't be done in OnEnable because it's apparently too early.
            if (rtStyle == null) {
                rtStyle = new GUIStyle(EditorStyles.foldout) {
                    richText = true
                };
            }
            
            using (new EditorGUI.DisabledGroupScope(true)) {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            }

            int oldLayer = layerField.intValue;
            int newLayer = EditorGUILayout.Popup("Physics Layer:", layerField.intValue, BetterPhysicsSettings.Instance.AllLayerNamesNumbered.ToArray());
            if (oldLayer != newLayer && serializedObject.targetObject is BetterRigidbody brb) {
                brb.PhysicsLayer = newLayer;
                layerField.intValue = newLayer;
            }

            DrawLimitsSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLimitsSection() {
            var limitsProperty = serializedObject.FindProperty("limits");
            EditorGUILayout.PropertyField(limitsProperty);
        }
    }
}
