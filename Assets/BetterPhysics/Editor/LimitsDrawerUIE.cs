using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SadnessMonday.BetterPhysics.Editor {
    [CustomPropertyDrawer(typeof(Limits))]
    public class LimitsDrawerUIE : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            float rowHeight = EditorGUIUtility.singleLineHeight;
            
            // Draw symmetry checkbox
            SerializedProperty symmetryProp = property.FindPropertyRelative("asymmetrical");
            Rect symmetryRect = new Rect(position.x, position.y, position.width, rowHeight);
            EditorGUI.PropertyField(symmetryRect, symmetryProp, new GUIContent("Asymmetrical"));
            
            // Calculate row positions
            Rect row1Rect = new Rect(position.x, position.y + rowHeight * 1, position.width, rowHeight);
            Rect row2Rect = new Rect(position.x, position.y + rowHeight * 2, position.width, rowHeight);
            Rect row3Rect = new Rect(position.x, position.y + rowHeight * 3, position.width, rowHeight);

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

            EditorGUI.EndProperty();
        }

        void DrawRow(Rect position, SerializedProperty limitedProp, SerializedProperty minProp, SerializedProperty maxProp, string label, int component, bool asymmetrical) {
            Rect checkboxRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, 20f, position.height);
            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth - 5f, position.height);
            Rect minValueRect = new Rect(position.x + EditorGUIUtility.labelWidth + 20f, position.y, (position.width - EditorGUIUtility.labelWidth) / (asymmetrical ? 2f : 1f) - 25f, position.height);
            Rect maxValueRect = asymmetrical ? new Rect(position.x + EditorGUIUtility.labelWidth + (position.width - EditorGUIUtility.labelWidth) / 2f + 5f, position.y, (position.width - EditorGUIUtility.labelWidth) / 2f - 25f, position.height) : Rect.zero;

            // Draw label
            EditorGUI.LabelField(labelRect, label);

            // Draw checkbox
            bool isActive = limitedProp.boolValue;
            isActive = EditorGUI.Toggle(checkboxRect, isActive);
            limitedProp.boolValue = isActive;

            if (!isActive) return;

            // Draw value field if checkbox is active
            // EditorGUI.BeginDisabledGroup(!isActive);
            Vector3 minValue = minProp.vector3Value;
            minValue[component] = EditorGUI.FloatField(minValueRect, GUIContent.none, minValue[component]);
            minProp.vector3Value = minValue;

            // Draw second value field if asymmetrical checkbox is enabled
            if (asymmetrical) {
                Vector3 maxValue = maxProp.vector3Value;
                maxValue[component] = EditorGUI.FloatField(maxValueRect, GUIContent.none, maxValue[component]);
                maxProp.vector3Value = maxValue;
            }

            // EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight * 4;
        }
        // public override VisualElement CreatePropertyGUI(SerializedProperty property)
        // {
        //     // Create a new property container.
        //     var container = new VisualElement();
        //
        //     // Add a label for the property.
        //     container.Add(new Label(property.displayName));
        //
        //     // Create fields for the symmetrical, min, and max properties.
        //     var symmetricalField = new PropertyField(property.FindPropertyRelative("symmetrical"));
        //     var minField = new PropertyField(property.FindPropertyRelative("min"));
        //     var maxField = new PropertyField(property.FindPropertyRelative("max"));
        //
        //     // Add the symmetrical, min, and max fields to the container.
        //     container.Add(symmetricalField);
        //     container.Add(minField);
        //     container.Add(maxField);
        //
        //     // Create fields for the xLimited, yLimited, and zLimited properties.
        //     var xLimitedField = new PropertyField(property.FindPropertyRelative("xLimited"));
        //     var yLimitedField = new PropertyField(property.FindPropertyRelative("yLimited"));
        //     var zLimitedField = new PropertyField(property.FindPropertyRelative("zLimited"));
        //
        //     // Add the xLimited, yLimited, and zLimited fields to the container.
        //     container.Add(xLimitedField);
        //     container.Add(yLimitedField);
        //     container.Add(zLimitedField);
        //
        //     // Return the container.
        //     return container;
        // }
        //
        // public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        //     return EditorGUIUtility.singleLineHeight * 5;
        // }

        // public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        //     EditorGUI.BeginProperty(position, label, property);
        //
        //     position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        //     Vector2 pos = position.position;
        //     
        //     var symmetricalProp = property.FindPropertyRelative("symmetrical");
        //     var xLimitedProp = property.FindPropertyRelative("xLimited");
        //     var yLimitedProp = property.FindPropertyRelative("yLimited");
        //     var zLimitedProp = property.FindPropertyRelative("zLimited");
        //     var minProp = property.FindPropertyRelative("min");
        //     var maxProp = property.FindPropertyRelative("max");
        //
        //     var symCheckboxRect = new Rect(position.x, position.y, position.width, position.height);
        //
        //
        //     Vector3 currentMin = minProp.vector3Value;
        //     Vector3 currentMax = maxProp.vector3Value;
        //     bool symmetrical = EditorGUI.ToggleLeft(symCheckboxRect, "Symmetrical", symmetricalProp.boolValue);
        //     symmetricalProp.boolValue = symmetrical;
        //
        //     var xRect = new Rect(pos.x, pos.y += EditorGUIUtility.singleLineHeight, position.width,
        //         EditorGUIUtility.singleLineHeight);
        //     var yRect = new Rect(pos.x, pos.y += EditorGUIUtility.singleLineHeight, position.width,
        //         EditorGUIUtility.singleLineHeight);
        //     var zRect = new Rect(pos.x, pos.y += EditorGUIUtility.singleLineHeight, position.width,
        //         EditorGUIUtility.singleLineHeight);
        //
        //     bool xLimited = EditorGUI.Toggle(xRect, "X", xLimitedProp.boolValue);
        //     bool yLimited = EditorGUI.Toggle(yRect, "Y", yLimitedProp.boolValue);
        //     bool zLimited = EditorGUI.Toggle(zRect, "Z", zLimitedProp.boolValue);
        //     
        //     if (!symmetrical) {
        //         // Show mins
        //         var maxRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, position.height);
        //         // symmetrical
        //         EditorGUI.PropertyField(maxRect, maxProp);
        //     }
        //     
        //     if (xLimited) {
        //         Rect xMaxRect = new Rect(xRect.position + Vector2.right * 50, xRect.size);
        //         currentMax.x = EditorGUI.DelayedFloatField(xMaxRect, currentMax.x);
        //
        //         if (!symmetrical) {
        //             Rect xMinRect = new Rect(xRect.position + Vector2.right * 20, xRect.size);
        //             currentMin.x = EditorGUI.DelayedFloatField(xMinRect, currentMin.x);
        //         }
        //     }
        //     
        //     if (yLimited) {
        //         Rect yMaxRect = new Rect(yRect.position + Vector2.right * 50, yRect.size);
        //         currentMax.y = EditorGUI.DelayedFloatField(yMaxRect, currentMax.y);
        //
        //         if (!symmetrical) {
        //             Rect yMinRect = new Rect(yRect.position + Vector2.right * 20, yRect.size);
        //             currentMin.y = EditorGUI.DelayedFloatField(yMinRect, currentMin.y);
        //         }
        //     }
        //     
        //     if (zLimited) {
        //         Rect zMaxRect = new Rect(zRect.position + Vector2.right * 50, zRect.size);
        //         currentMax.z = EditorGUI.DelayedFloatField(zMaxRect, currentMax.z);
        //
        //         if (!symmetrical) {
        //             Rect zMinRect = new Rect(zRect.position + Vector2.right * 20, zRect.size);
        //             currentMin.z = EditorGUI.DelayedFloatField(zMinRect, currentMin.z);
        //         }
        //     }
        //
        //     minProp.vector3Value = currentMin;
        //     maxProp.vector3Value = currentMax;
        //     
        //     EditorGUI.EndProperty();
        // }
    }
}