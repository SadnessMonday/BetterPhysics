using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SadnessMonday.BetterPhysics;
using SadnessMonday.BetterPhysics.Layers;
using UnityEngine;
using UnityEditor;

namespace SadnessMonday.BetterPhysics.Editor {

    [CustomEditor(typeof(BetterRigidbody))]
    public class BetterRigidbodyEditor : UnityEditor.Editor
    {
        private static GUIContent LimitContent = new GUIContent("Limit:", "Negative means unlimited.");
        private bool showAdvanced;

        public override void OnInspectorGUI()
        {
            var brb = (BetterRigidbody)target;
            var serializedObject = new SerializedObject(brb);
            var softLimitTypeProp = serializedObject.FindProperty("SoftLimitType");

            using (new EditorGUI.DisabledGroupScope(true)) {
                var scriptAsset = (MonoScript)EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(brb), typeof(MonoScript), false);
            }

            var layerField = serializedObject.FindProperty("physicsLayer");
            layerField.intValue = EditorGUILayout.Popup("Physics Layer:", layerField.intValue, BetterPhysicsSettings.Instance.AllLayerNames.ToArray());

            var softLimitField = serializedObject.FindProperty("softLimitType");
            LimitType softLimitType = (LimitType)EditorGUILayout.EnumPopup("Soft Limit Type:", (LimitType)softLimitField.enumValueIndex);
            softLimitField.enumValueIndex = (int)softLimitType;
            switch(softLimitType) {
                case LimitType.None:
                    break;
                case LimitType.Omnidirectional:
                    var softLimitScalarField = serializedObject.FindProperty("softScalarLimit");
                    softLimitScalarField.floatValue = EditorGUILayout.FloatField(LimitContent, softLimitScalarField.floatValue);
                    break;
                case LimitType.LocalAxes:
                    // fallthrough
                case LimitType.WorldAxes:
                    var softLimitVectorField = serializedObject.FindProperty("softVectorLimit");
                    softLimitVectorField.vector3Value = EditorGUILayout.Vector3Field(LimitContent, softLimitVectorField.vector3Value);
                    break;
            }

            var hardLimitField = serializedObject.FindProperty("hardLimitType");
            LimitType hardLimitType = (LimitType)EditorGUILayout.EnumPopup("Hard Limit Type:", (LimitType)hardLimitField.enumValueIndex);
            hardLimitField.enumValueIndex = (int)hardLimitType;
            switch(hardLimitType) {
                case LimitType.None:
                    break;
                case LimitType.Omnidirectional:
                    var hardLimitScalarField = serializedObject.FindProperty("hardScalarLimit");
                    hardLimitScalarField.floatValue = EditorGUILayout.FloatField(LimitContent, hardLimitScalarField.floatValue);
                    break;
                case LimitType.LocalAxes:
                // fallthrough
                case LimitType.WorldAxes:
                    var hardLimitVectorField = serializedObject.FindProperty("hardVectorLimit");
                    hardLimitVectorField.vector3Value = EditorGUILayout.Vector3Field(LimitContent, hardLimitVectorField.vector3Value);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
