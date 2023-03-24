using System.Linq;
using System.Reflection;
using SadnessMonday.BetterPhysics.Layers;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

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
        
        #region wrapped rb

        private SerializedObject rbObj;
        
        SerializedProperty m_Constraints;
        SerializedProperty m_Mass;
        SerializedProperty m_Drag;
        SerializedProperty m_AngularDrag;

        SerializedProperty m_ImplicitCom;
        SerializedProperty m_CenterOfMass;
        SerializedProperty m_ImplicitTensor;
        SerializedProperty m_InertiaTensor;
        SerializedProperty m_InertiaRotation;

        SerializedProperty m_UseGravity;
        SerializedProperty m_IsKinematic;
        SerializedProperty m_Interpolate;
        SerializedProperty m_CollisionDetection;

        readonly AnimBool m_ShowLayerOverrides = new();
        private bool m_ShowLayerOverridesFoldout;
        SerializedProperty m_IncludeLayers;
        SerializedProperty m_ExcludeLayers;
        #endregion

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
            public static GUIContent mass = EditorGUIUtility.TrTextContent("Mass", "Mass of this rigid body.");
            public static GUIContent useGravity = EditorGUIUtility.TrTextContent("Use Gravity", "Controls whether gravity affects this rigid body.");

            public static GUIContent drag = EditorGUIUtility.TrTextContent("Drag", "Damping factor that affects how this body resists linear motion.");
            public static GUIContent angularDrag = EditorGUIUtility.TrTextContent("Angular Drag", "Damping factor that affects how this body resists rotations.");
            public static GUIContent isKinematic = EditorGUIUtility.TrTextContent("Is Kinematic", "Controls whether physics affects the rigidbody.");
            public static GUIContent interpolate = EditorGUIUtility.TrTextContent("Interpolate", "Smooths out the effect of running physics at a fixed frame rate.");

            public static GUIContent implicitCom = EditorGUIUtility.TrTextContent("Automatic Center Of Mass", "Use the calculated center of mass or set it directly.");
            public static GUIContent implicitTensor = EditorGUIUtility.TrTextContent("Automatic Tensor", "Use the calculated tensor or set it directly.");
            public static GUIContent centerOfMass = EditorGUIUtility.TrTextContent("Center Of Mass", "The local space coordinates of the center of mass.");
            public static GUIContent inertiaTensor = EditorGUIUtility.TrTextContent("Inertia Tensor", "The diagonal inertia tensor of mass relative to the center of mass.");
            public static GUIContent inertiaRotation = EditorGUIUtility.TrTextContent("Inertia Tensor Rotation", "The rotation of the inertia tensor.");

            public static GUIContent collisionDetection = EditorGUIUtility.TrTextContent("Collision Detection", "The method to use to detect collisions for child colliders: discrete (default) or various modes of continuous collision detection that can help solving fast moving object issues.");

            public static GUIContent freezePositionLabel = EditorGUIUtility.TrTextContent("Freeze Position");
            public static GUIContent freezeRotationLabel = EditorGUIUtility.TrTextContent("Freeze Rotation");

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
            rbObj = new SerializedObject(((Component)target).GetComponent<Rigidbody>());

            // wrapped rb stuff
            m_Mass = rbObj.FindProperty("m_Mass");
            m_Drag = rbObj.FindProperty("m_Drag");
            m_AngularDrag = rbObj.FindProperty("m_AngularDrag");

            m_ImplicitCom = rbObj.FindProperty("m_ImplicitCom");
            m_CenterOfMass = rbObj.FindProperty("m_CenterOfMass");
            m_ImplicitTensor = rbObj.FindProperty("m_ImplicitTensor");
            m_InertiaTensor = rbObj.FindProperty("m_InertiaTensor");
            m_InertiaRotation = rbObj.FindProperty("m_InertiaRotation");

            m_UseGravity = rbObj.FindProperty("m_UseGravity");
            m_IsKinematic = rbObj.FindProperty("m_IsKinematic");
            m_Interpolate = rbObj.FindProperty("m_Interpolate");
            m_CollisionDetection = rbObj.FindProperty("m_CollisionDetection");
            m_Constraints = rbObj.FindProperty("m_Constraints");

            m_IncludeLayers = rbObj.FindProperty("m_IncludeLayers");
            m_ExcludeLayers = rbObj.FindProperty("m_ExcludeLayers");

            m_ShowLayerOverrides.valueChanged.AddListener(Repaint);
            m_ShowLayerOverridesFoldout = false;
            m_ShowLayerOverrides.value = m_ShowLayerOverridesFoldout;
        }

        private void OnDisable() {
            m_ShowLayerOverrides.valueChanged.RemoveListener(Repaint);
        }
        
        void ConstraintToggle(Rect r, string label, RigidbodyConstraints value, int bit)
        {
            bool toggle = ((int)value & (1 << bit)) != 0;

            int hasMultipleValues = (int) hasMultipleDifferentValuesBitwise.GetValue(m_Constraints);
            EditorGUI.showMixedValue = (hasMultipleValues & (1 << bit)) != 0;
            EditorGUI.BeginChangeCheck();
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            toggle = EditorGUI.ToggleLeft(r, label, toggle);
            EditorGUI.indentLevel = oldIndent;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(targets, "Edit Constraints");
                
                // m_Constraints.SetBitAtIndexForAllTargetsImmediate(bit, toggle);
                SetBitAtIndexForAllTargetsImmediate.Invoke(m_Constraints, new object[] { bit, toggle });
            }
            EditorGUI.showMixedValue = false;
        }

        void ToggleBlock(RigidbodyConstraints constraints, GUIContent label, int x, int y, int z)
        {
            const int toggleOffset = 30;
            GUILayout.BeginHorizontal();
            float maxW = (float)kLabelFloatMaxW.GetValue(null, null);
            float height = (float)kSingleLineHeight.GetRawConstantValue();
            Rect r = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, maxW, height, height, EditorStyles.numberField);
            int id = GUIUtility.GetControlID(7231, FocusType.Keyboard, r);
            r = EditorGUI.PrefixLabel(r, id, label);
            r.width = toggleOffset;
            ConstraintToggle(r, "X", constraints, x);
            r.x += toggleOffset;
            ConstraintToggle(r, "Y", constraints, y);
            r.x += toggleOffset;
            ConstraintToggle(r, "Z", constraints, z);
            GUILayout.EndHorizontal();
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

            showBaseRigidbodySettings = EditorGUILayout.Foldout(showBaseRigidbodySettings, "Base Rigidbody Settings");
            if (showBaseRigidbodySettings) {
                rbObj.Update();

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_Mass, Styles.mass);
                EditorGUILayout.PropertyField(m_Drag, Styles.drag);
                EditorGUILayout.PropertyField(m_AngularDrag, Styles.angularDrag);
                EditorGUILayout.PropertyField(m_ImplicitCom, Styles.implicitCom);
                if (!m_ImplicitCom.boolValue)
                    EditorGUILayout.PropertyField(m_CenterOfMass, Styles.centerOfMass);
                EditorGUILayout.PropertyField(m_ImplicitTensor, Styles.implicitTensor);
                if (!m_ImplicitTensor.boolValue)
                {
                    EditorGUILayout.PropertyField(m_InertiaTensor, Styles.inertiaTensor);
                    EditorGUILayout.PropertyField(m_InertiaRotation, Styles.inertiaRotation);
                }

                EditorGUILayout.PropertyField(m_UseGravity, Styles.useGravity);
                EditorGUILayout.PropertyField(m_IsKinematic, Styles.isKinematic);
                EditorGUILayout.PropertyField(m_Interpolate, Styles.interpolate);
                EditorGUILayout.PropertyField(m_CollisionDetection, Styles.collisionDetection);

                if (targets.Any(x => (x as BetterRigidbody).GetComponent<Rigidbody>().interpolation != RigidbodyInterpolation.None))
                {
                    if (Physics.simulationMode == SimulationMode.Update)
                        EditorGUILayout.HelpBox("The physics simulation mode is set to run per-frame. Any interpolation mode will be ignored and can be set to 'None'.", MessageType.Info);
                    else if (Physics.simulationMode == SimulationMode.Script)
                        EditorGUILayout.HelpBox("The physics simulation mode is set to run manually in the scripts. Some or all selected Rigidbodies are using an interpolation mode other than 'None' which will be executed per-frame. If the manual simulation is being run per-frame then the interpolation mode should be set to 'None'.", MessageType.Info);
                }

                Rect position = EditorGUILayout.GetControlRect();
                EditorGUI.BeginProperty(position, null, m_Constraints);
                m_Constraints.isExpanded = EditorGUI.Foldout(position, m_Constraints.isExpanded, m_Constraints.displayName, true);
                EditorGUI.EndProperty();

                RigidbodyConstraints constraints = (RigidbodyConstraints)m_Constraints.intValue;
                if (m_Constraints.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    ToggleBlock(constraints, Styles.freezePositionLabel, 1, 2, 3);
                    ToggleBlock(constraints, Styles.freezeRotationLabel, 4, 5, 6);
                    EditorGUI.indentLevel--;
                }
                
                ShowLayerOverridesProperties();
                
                EditorGUI.indentLevel--;
                rbObj.ApplyModifiedProperties();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLimitsSection() {
            var softLimitsProperty = serializedObject.FindProperty("softLimits");
            var hardLimitsProperty = serializedObject.FindProperty("hardLimits");

            string limitsFoldoutText = SpeedLimitsLabelPrefix;
            if (!showSpeedLimits) {
                // potentially show extra info
                Limits softLimits = (Limits)softLimitsProperty.boxedValue;
                Limits hardLimits = (Limits)hardLimitsProperty.boxedValue;
                bool hasSoftLimit = softLimits.LimitType != LimitType.None;
                bool hasHardLimit = hardLimits.LimitType != LimitType.None;
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
                EditorGUILayout.PropertyField(softLimitsProperty);
                EditorGUILayout.PropertyField(hardLimitsProperty);
            }
        }

        private void ShowLayerOverridesProperties()
        {
            // Show Layer Overrides.
            m_ShowLayerOverridesFoldout = m_ShowLayerOverrides.target = EditorGUILayout.Foldout(m_ShowLayerOverrides.target, "Layer Overrides", true);
            if (EditorGUILayout.BeginFadeGroup(m_ShowLayerOverrides.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_IncludeLayers, Styles.includeLayers);
                EditorGUILayout.PropertyField(m_ExcludeLayers, Styles.excludeLayers);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndFadeGroup();
        }
    }
}
