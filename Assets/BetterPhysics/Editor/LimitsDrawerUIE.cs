using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SadnessMonday.BetterPhysics.Editor {
    [CustomPropertyDrawer(typeof(Limits))]
    public class LimitsDrawerUIE : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight * 5;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            Vector2 pos = position.position;
            
            var symmetricalProp = property.FindPropertyRelative("symmetrical");
            var xLimitedProp = property.FindPropertyRelative("xLimited");
            var yLimitedProp = property.FindPropertyRelative("yLimited");
            var zLimitedProp = property.FindPropertyRelative("zLimited");
            var minProp = property.FindPropertyRelative("min");
            var maxProp = property.FindPropertyRelative("max");

            var symCheckboxRect = new Rect(position.x, position.y, position.width, position.height);


            Vector3 currentMin = minProp.vector3Value;
            Vector3 currentMax = maxProp.vector3Value;
            bool symmetrical = EditorGUI.ToggleLeft(symCheckboxRect, "Symmetrical", symmetricalProp.boolValue);
            symmetricalProp.boolValue = symmetrical;

            var xRect = new Rect(pos.x, pos.y += EditorGUIUtility.singleLineHeight, position.width,
                EditorGUIUtility.singleLineHeight);
            var yRect = new Rect(pos.x, pos.y += EditorGUIUtility.singleLineHeight, position.width,
                EditorGUIUtility.singleLineHeight);
            var zRect = new Rect(pos.x, pos.y += EditorGUIUtility.singleLineHeight, position.width,
                EditorGUIUtility.singleLineHeight);

            bool xLimited = EditorGUI.Toggle(xRect, "X", xLimitedProp.boolValue);
            bool yLimited = EditorGUI.Toggle(yRect, "Y", yLimitedProp.boolValue);
            bool zLimited = EditorGUI.Toggle(zRect, "Z", zLimitedProp.boolValue);
            
            if (!symmetrical) {
                // Show mins
                var maxRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, position.height);
                // symmetrical
                EditorGUI.PropertyField(maxRect, maxProp);
            }
            
            if (xLimited) {
                Rect xMaxRect = new Rect(xRect.position + Vector2.right * 50, xRect.size);
                currentMax.x = EditorGUI.DelayedFloatField(xMaxRect, currentMax.x);

                if (!symmetrical) {
                    Rect xMinRect = new Rect(xRect.position + Vector2.right * 20, xRect.size);
                    currentMin.x = EditorGUI.DelayedFloatField(xMinRect, currentMin.x);
                }
            }
            
            if (yLimited) {
                Rect yMaxRect = new Rect(yRect.position + Vector2.right * 50, yRect.size);
                currentMax.y = EditorGUI.DelayedFloatField(yMaxRect, currentMax.y);

                if (!symmetrical) {
                    Rect yMinRect = new Rect(yRect.position + Vector2.right * 20, yRect.size);
                    currentMin.y = EditorGUI.DelayedFloatField(yMinRect, currentMin.y);
                }
            }
            
            if (zLimited) {
                Rect zMaxRect = new Rect(zRect.position + Vector2.right * 50, zRect.size);
                currentMax.z = EditorGUI.DelayedFloatField(zMaxRect, currentMax.z);

                if (!symmetrical) {
                    Rect zMinRect = new Rect(zRect.position + Vector2.right * 20, zRect.size);
                    currentMin.z = EditorGUI.DelayedFloatField(zMinRect, currentMin.z);
                }
            }

            minProp.vector3Value = currentMin;
            maxProp.vector3Value = currentMax;
            
            EditorGUI.EndProperty();
        }
    }
}