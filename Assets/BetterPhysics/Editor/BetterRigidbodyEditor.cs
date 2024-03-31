using System.Linq;
using System.Reflection;
using SadnessMonday.BetterPhysics.Layers;
using UnityEngine;
using UnityEditor;

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
        private bool showSpeedLimits = false;

        private SerializedProperty layerField;
        private SerializedProperty softLimitField;
        private SerializedProperty hardLimitField;
        private SerializedProperty softLimitScalarField;
        private SerializedProperty hardLimitScalarField;
        private SerializedProperty softLimitVectorField;
        private SerializedProperty hardLimitVectorField;

        private static readonly MethodInfo SetBitAtIndexForAllTargetsImmediate;
        private static readonly PropertyInfo hasMultipleDifferentValuesBitwise;
        private static readonly PropertyInfo kLabelFloatMaxW;
        private static readonly FieldInfo kSingleLineHeight;
        
        static BetterRigidbodyEditor() {
            SetBitAtIndexForAllTargetsImmediate =
                typeof(SerializedProperty).GetMethod("SetBitAtIndexForAllTargetsImmediate", BindingFlags.Instance | BindingFlags.NonPublic);
            hasMultipleDifferentValuesBitwise = typeof(SerializedProperty).GetProperty("hasMultipleDifferentValuesBitwise", BindingFlags.Instance | BindingFlags.NonPublic);
            kLabelFloatMaxW =
                typeof(EditorGUILayout).GetProperty("kLabelFloatMaxW", BindingFlags.Static | BindingFlags.NonPublic);
            kSingleLineHeight =
                typeof(EditorGUI).GetField("kSingleLineHeight", BindingFlags.Static | BindingFlags.NonPublic);
            SerializedProperty p;
        }
        
        private class Styles
        {
            public static GUIContent includeLayers = EditorGUIUtility.TrTextContent("Include Layers", "Layers to include when producing collisions");
            public static GUIContent excludeLayers = EditorGUIUtility.TrTextContent("Exclude Layers", "Layers to exclude when producing collisions");
        }

        private void OnEnable() {
            layerField = serializedObject.FindProperty("physicsLayer");
            softLimitField = serializedObject.FindProperty("softLimitType");

            hardLimitField = serializedObject.FindProperty("hardLimitType");
            softLimitScalarField = serializedObject.FindProperty("softScalarLimit");
            hardLimitScalarField = serializedObject.FindProperty("hardScalarLimit");
            softLimitVectorField = serializedObject.FindProperty("softVectorLimit");
            hardLimitVectorField = serializedObject.FindProperty("hardVectorLimit");
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
            
            layerField.intValue = EditorGUILayout.Popup("Physics Layer:", layerField.intValue, BetterPhysicsSettings.Instance.AllLayerNames.ToArray());
            DrawLimitsSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLimitsSection() {
            var limitsProperty = serializedObject.FindProperty("limits");

            string limitsFoldoutText = SpeedLimitsLabelPrefix;
            if (!showSpeedLimits) {
                // potentially show extra info
                bool hasSoftLimit = false;
                bool hasHardLimit = false;
                for (int i = 0; i < limitsProperty.arraySize; i++) {
                    SerializedProperty limitProp = limitsProperty.GetArrayElementAtIndex(i);
                    var limit = (SpeedLimit)limitProp.boxedValue;
                    hasSoftLimit |= limit.LimitType == LimitType.Soft;
                    hasHardLimit |= limit.LimitType == LimitType.Hard;
                    
                    if (hasSoftLimit && hasHardLimit) break;
                }
                
                if (hasSoftLimit && hasHardLimit) {
                    limitsFoldoutText = BothLimits;
                }
                else if (hasSoftLimit) {
                    limitsFoldoutText = SoftLimitOnly;
                }
                else if (hasHardLimit) {
                    limitsFoldoutText = HardLimitOnly;
                }
            }
            
            showSpeedLimits = EditorGUILayout.Foldout(showSpeedLimits, limitsFoldoutText, rtStyle);
            if (showSpeedLimits) {
                EditorGUILayout.PropertyField(limitsProperty);
            }
        }
    }
}
