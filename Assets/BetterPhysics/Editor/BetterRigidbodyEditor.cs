using System.Collections;
using System.Collections.Generic;
using SadnessMonday.BetterPhysics;
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

            // Draw the default inspector
            EditorGUI.BeginChangeCheck();

            brb.SoftLimitType = (LimitType)EditorGUILayout.EnumPopup("Soft Limit Type:", brb.SoftLimitType);
            switch(brb.SoftLimitType) {
                case LimitType.None:
                    break;
                case LimitType.Omnidirectional:
                    brb.SoftScalarLimit = EditorGUILayout.FloatField(LimitContent, brb.SoftScalarLimit);
                    break;
                case LimitType.LocalAxes:
                    // fallthrough
                case LimitType.WorldAxes:
                    brb.SoftVectorLimit = EditorGUILayout.Vector3Field(LimitContent, brb.SoftVectorLimit);
                    break;
            }

            brb.HardLimitType = (LimitType)EditorGUILayout.EnumPopup("Hard Limit Type:", brb.HardLimitType);
            switch(brb.HardLimitType) {
                case LimitType.None:
                    break;
                case LimitType.Omnidirectional:
                    brb.HardScalarLimit = EditorGUILayout.FloatField(LimitContent, brb.HardScalarLimit);
                    break;
                case LimitType.LocalAxes:
                    // fallthrough
                case LimitType.WorldAxes:
                    brb.HardVectorLimit = EditorGUILayout.Vector3Field(LimitContent, brb.HardVectorLimit);
                    break;
            }

            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(target);
            }
        }
    }
}
