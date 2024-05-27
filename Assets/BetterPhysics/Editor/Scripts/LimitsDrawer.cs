using System;
using UnityEditor;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Editor {
    [CustomPropertyDrawer(typeof(SpeedLimit))]
    public class LimitsDrawer : PropertyDrawer {
        private const float LabelWidth = 40;
        const float Between = 5;
        const float Buffer = 20;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            float rowHeight = EditorGUIUtility.singleLineHeight;
            float posY = position.y;

            // Draw Limit type field
            SerializedProperty limitTypeProp = property.FindPropertyRelative("limitType");
            SerializedProperty directionalityProp = property.FindPropertyRelative("directionality");
            Rect limitTypeRect = new(position.x, posY, position.width, rowHeight);
            EditorGUI.PropertyField(limitTypeRect, limitTypeProp, new GUIContent("Limit Type"));
            LimitType limitType = (LimitType)limitTypeProp.enumValueIndex;
            
            if (limitType != LimitType.None) {
                posY += rowHeight;
                position.y += rowHeight;
                Rect directionalityRect = new(position.x, posY, position.width, rowHeight);
                EditorGUI.PropertyField(directionalityRect, directionalityProp, new GUIContent("Directionality"));
            
                Directionality directionality = (Directionality)directionalityProp.enumValueIndex;

                switch (directionality) {
                    case Directionality.Omnidirectional:
                        DrawScalarLimit(property, position, posY);
                        break;
                    case Directionality.WorldAxes:
                    case Directionality.LocalAxes:
                        DrawVectorLimit(property, position, posY);
                        break;
                }
            }


            EditorGUI.EndProperty();
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
            SerializedProperty asymmetricalProp = property.FindPropertyRelative("asymmetrical");
            Rect asymmetricalRect = new Rect(position.x, posY += rowHeight, position.width, rowHeight);
            EditorGUI.PropertyField(asymmetricalRect, asymmetricalProp, new GUIContent("Asymmetrical"));
            bool asymmetrical = asymmetricalProp.boolValue;

            Rect HeadersRect = new Rect(position.x, posY += rowHeight, position.width, rowHeight);
            DrawHeaders(HeadersRect, asymmetrical);
            
            // Calculate row positions
            Rect row1Rect = new Rect(position.x, posY += rowHeight, position.width, rowHeight);
            Rect row2Rect = new Rect(position.x, posY += rowHeight, position.width, rowHeight);
            Rect row3Rect = new Rect(position.x, posY += rowHeight, position.width, rowHeight);

            SerializedProperty minProp = property.FindPropertyRelative("min");
            SerializedProperty maxProp = property.FindPropertyRelative("max");
            
            // Draw rows
            SerializedProperty axisLimitedProp = property.FindPropertyRelative("axisLimited");
            DrawRow(row1Rect, axisLimitedProp, minProp, maxProp, "x", 0, asymmetrical);
            DrawRow(row2Rect, axisLimitedProp, minProp, maxProp, "y", 1, asymmetrical);
            DrawRow(row3Rect, axisLimitedProp, minProp, maxProp,"z", 2, asymmetrical);
        }

        private void DrawHeaders(Rect headersRect, bool asymmetrical) {
            Rect labelRect = GetAxisLabelRect(headersRect);
            
            Rect minValueRect;
            Rect maxValueRect;
            float remainingSpace = headersRect.width - LabelWidth;
            float halfRemainingSpace = (remainingSpace - Between) / 2;
            if (asymmetrical) {
                minValueRect = new Rect(labelRect.xMax + Buffer, headersRect.y,  halfRemainingSpace, headersRect.height);
                maxValueRect =
                    new Rect(
                        minValueRect.xMax + Between, headersRect.y, halfRemainingSpace, headersRect.height);
            }
            else {
                minValueRect = Rect.zero;
                maxValueRect = new Rect(labelRect.xMax + Buffer, headersRect.y,
                    remainingSpace - Buffer, headersRect.height);
            }
            
            EditorGUI.LabelField(minValueRect, "Minimum");
            EditorGUI.LabelField(maxValueRect, "Maximum");
        }

        Rect GetAxisLabelRect(Rect containingRect) {
            Rect labelRect = new Rect(containingRect.x, containingRect.y, LabelWidth, containingRect.height);
            return labelRect;
        }
        
        void DrawRow(Rect position, SerializedProperty axisLimitedProp, SerializedProperty minProp, SerializedProperty maxProp, string label, int component, bool asymmetrical) {
            Rect labelRect = GetAxisLabelRect(position);
            // Rect checkboxRect = new Rect(position.x + labelWidth, position.y, 20f, position.height);

            float remainingSpace = position.width - LabelWidth;
            float halfRemainingSpace = (remainingSpace - Between) / 2;
            Rect minValueRect;
            Rect maxValueRect;
            if (asymmetrical) {
                minValueRect = new Rect(labelRect.xMax + Buffer, position.y,  halfRemainingSpace, position.height);
                maxValueRect =
                    new Rect(
                        minValueRect.xMax + Between, position.y, halfRemainingSpace, position.height);
            }
            else {
                minValueRect = Rect.zero;
                maxValueRect = new Rect(labelRect.xMax + Buffer, position.y,
                    remainingSpace - Buffer, position.height);
            }

            // Draw label
            // EditorGUI.LabelField(labelRect, label);

            // Draw checkbox
            Bool3 isActive = (Bool3)axisLimitedProp.boxedValue;
            isActive[component] = EditorGUI.ToggleLeft(labelRect, label, isActive[component]);
            axisLimitedProp.boxedValue = isActive;

            if (!isActive[component]) return;

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
        
        private const int OmnidirectionalLines = 3;
        private const int WorldAxesLines = 7;
        private const int LocalAxesLines = WorldAxesLines;

        private static float OmnidirectionalHeight => EditorGUIUtility.singleLineHeight * OmnidirectionalLines;
        private static float WorldAxesHeight => EditorGUIUtility.singleLineHeight * WorldAxesLines;
        private static float LocalAxesHeight => EditorGUIUtility.singleLineHeight * LocalAxesLines;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            SerializedProperty limitTypeProp = property.FindPropertyRelative("limitType");
            LimitType limitType = (LimitType)limitTypeProp.enumValueIndex;

            if (limitType == LimitType.None) {
                return EditorGUIUtility.singleLineHeight * 1;
            }
            
            // Draw Limit type field
            SerializedProperty directionalityProp = property.FindPropertyRelative("directionality");
            Directionality directionality = (Directionality)directionalityProp.enumValueIndex;
            switch (directionality) {
                case Directionality.Omnidirectional:
                    return OmnidirectionalHeight;
                case Directionality.WorldAxes:
                    return WorldAxesHeight;
                case Directionality.LocalAxes:
                    return LocalAxesHeight;
                default:
                    throw new Exception($"Unknown directionality {directionality}");
            }
        }
    }
}