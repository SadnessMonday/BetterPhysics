using System;
using UnityEditor;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Editor {
    [CustomPropertyDrawer(typeof(Limits))]
    public class LimitsDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            float rowHeight = EditorGUIUtility.singleLineHeight;
            float posY = position.y;

            // Draw Limit type field
            SerializedProperty limitTypeProp = property.FindPropertyRelative("limitType");
            Rect limitTypeRect = new(position.x, posY, position.width, rowHeight);
            EditorGUI.PropertyField(limitTypeRect, limitTypeProp, new GUIContent("Limit Type"));
            
            LimitType limitType = (LimitType)limitTypeProp.enumValueIndex;
            switch (limitType) {
                case LimitType.None:
                    DrawNothing(property, position, posY);
                    break;
                case LimitType.Omnidirectional:
                    DrawScalarLimit(property, position, posY);
                    break;
                case LimitType.WorldAxes:
                case LimitType.LocalAxes:
                    DrawVectorLimit(property, position, posY);
                    break;
            }


            EditorGUI.EndProperty();
        }

        private void DrawNothing(SerializedProperty property, Rect position, float posY) {
            float rowHeight = EditorGUIUtility.singleLineHeight;
        }

        private void DrawScalarLimit(SerializedProperty property, Rect position, float posY) {
            float rowHeight = EditorGUIUtility.singleLineHeight;
            SerializedProperty scalarLimitProperty = property.FindPropertyRelative("scalarLimit");
            Rect scalarRect = new Rect(position.x, posY += rowHeight, position.width, rowHeight);
            EditorGUI.PropertyField(scalarRect, scalarLimitProperty, new GUIContent("Limit"));
        }

        private void DrawVectorLimit(SerializedProperty property, Rect position, float posY) {
            float rowHeight = EditorGUIUtility.singleLineHeight;
            
            // Draw symmetry checkbox
            SerializedProperty symmetryProp = property.FindPropertyRelative("asymmetrical");
            Rect symmetryRect = new Rect(position.x, posY += rowHeight, position.width, rowHeight);
            EditorGUI.PropertyField(symmetryRect, symmetryProp, new GUIContent("Asymmetrical"));
            
            // Calculate row positions
            Rect row1Rect = new Rect(position.x, posY += rowHeight, position.width, rowHeight);
            Rect row2Rect = new Rect(position.x, posY += rowHeight, position.width, rowHeight);
            Rect row3Rect = new Rect(position.x, posY += rowHeight, position.width, rowHeight);

            SerializedProperty minProp = property.FindPropertyRelative("min");
            SerializedProperty maxProp = property.FindPropertyRelative("max");
            bool symmetrical = symmetryProp.boolValue;
            
            // Draw rows
            SerializedProperty xLimitedProp = property.FindPropertyRelative("xLimited");
            SerializedProperty yLimitedProp = property.FindPropertyRelative("yLimited");
            SerializedProperty zLimitedProp = property.FindPropertyRelative("zLimited");
            DrawRow(row1Rect, xLimitedProp, minProp, maxProp, "x", 0, symmetrical);
            DrawRow(row2Rect, yLimitedProp, minProp, maxProp, "y", 1, symmetrical);
            DrawRow(row3Rect, zLimitedProp, minProp, maxProp,"z", 2, symmetrical);
        }

        void DrawRow(Rect position, SerializedProperty limitedProp, SerializedProperty minProp, SerializedProperty maxProp, string label, int component, bool asymmetrical) {
            Rect checkboxRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, 20f, position.height);
            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth - 5f, position.height);

            Rect minValueRect;
            Rect maxValueRect;
            if (asymmetrical) {
                minValueRect = new Rect(position.x + EditorGUIUtility.labelWidth + 20f, position.y, (position.width - EditorGUIUtility.labelWidth) / 2f - 25f, position.height);
                maxValueRect =
                    new Rect(
                        position.x + EditorGUIUtility.labelWidth + (position.width - EditorGUIUtility.labelWidth) / 2f +
                        5f, position.y, (position.width - EditorGUIUtility.labelWidth) / 2f - 25f, position.height);
            }
            else {
                minValueRect = Rect.zero;
                maxValueRect = new Rect(position.x + EditorGUIUtility.labelWidth + 20f, position.y,
                    position.width - EditorGUIUtility.labelWidth - 25f, position.height);
            }

            // Draw label
            EditorGUI.LabelField(labelRect, label);

            // Draw checkbox
            bool isActive = limitedProp.boolValue;
            isActive = EditorGUI.Toggle(checkboxRect, isActive);
            limitedProp.boolValue = isActive;

            if (!isActive) return;

            // Draw second value field if asymmetrical checkbox is enabled
            if (asymmetrical) {
                Vector3 minValue = minProp.vector3Value;
                minValue[component] = EditorGUI.FloatField(minValueRect, GUIContent.none, minValue[component]);
                minProp.vector3Value = minValue;
                
                Vector3 maxValue = maxProp.vector3Value;
                maxValue[component] = EditorGUI.FloatField(maxValueRect, GUIContent.none, maxValue[component]);
                maxProp.vector3Value = maxValue;
            }
            else {
                Vector3 maxValue = maxProp.vector3Value;
                // Take absolute value here. We only allow positives for symmetrical.
                maxValue[component] =
                    Mathf.Abs(EditorGUI.FloatField(maxValueRect, GUIContent.none, maxValue[component]));
                maxProp.vector3Value = maxValue;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            // Draw Limit type field
            SerializedProperty limitTypeProp = property.FindPropertyRelative("limitType");
            LimitType limitType = (LimitType)limitTypeProp.enumValueIndex;
            switch (limitType) {
                case LimitType.None:
                    return EditorGUIUtility.singleLineHeight * 1;
                case LimitType.Omnidirectional:
                    return EditorGUIUtility.singleLineHeight * 2;
                case LimitType.WorldAxes:
                case LimitType.LocalAxes:
                    return EditorGUIUtility.singleLineHeight * 5;
                default:
                    return EditorGUIUtility.singleLineHeight * 1;
            }
        }
    }
}