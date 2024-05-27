using System;
using System.Reflection;
using SadnessMonday.BetterPhysics.Layers;
using UnityEditor;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Editor {
    internal class LayerInteractionMatrixGUI {
        // const string IMGuiAssemblyName = "UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        private const string GuiClipAQN =
            "UnityEngine.GUIClip, UnityEngine.IMGUIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        private const string GUIVIewAQN =
            "UnityEditor.GUIView, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        private static readonly PropertyInfo topMostRectProp;
        private static readonly MethodInfo UnclipMethod;

        private static readonly PropertyInfo guiViewCurrentProp;
        private static readonly MethodInfo MarkHotRegionMethod;

        private static readonly Color DefaultInteractionColor = Color.grey;
        private static readonly Color FeatherInteractionColor = Color.yellow;
        private static readonly Color KinematicInteractionColor = Color.green;
        private static readonly int InteractionTypeCount;

        static LayerInteractionMatrixGUI() {
            // var imGUI = typeof(GUILayout).Assembly;

            var guiClip = Type.GetType(GuiClipAQN);
            topMostRectProp = guiClip.GetProperty("topmostRect", BindingFlags.Static | BindingFlags.NonPublic);
            UnclipMethod = guiClip.GetMethod("Unclip", BindingFlags.Static | BindingFlags.Public, Type.DefaultBinder,
                new[] { typeof(Vector2) }, null);

            var guiView = Type.GetType(GUIVIewAQN); //
            guiViewCurrentProp = guiView.GetProperty("current", BindingFlags.Static | BindingFlags.Public);
            MarkHotRegionMethod = guiView.GetMethod("MarkHotRegion", BindingFlags.Instance | BindingFlags.NonPublic);

            InteractionTypeCount = Enum.GetNames(typeof(InteractionType)).Length;
        }

        static class Styles {
            public static readonly GUIStyle rightLabel = new GUIStyle("RightLabel");
            public static readonly GUIStyle hoverStyle = GetHoverStyle();
        }

        private static Color transparentColor = new Color(1, 1, 1, 0);

        private static Color highlightColor =
            EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.2f) : new Color(0, 0, 0, 0.2f);

        public delegate InteractionType GetValueFunc(int layerA, int layerB);

        public delegate void SetValueFunc(int layerA, int layerB, InteractionType val);

        // Get the styled used when hovering over the rows/columns.
        public static GUIStyle GetHoverStyle() {
            GUIStyle style = new GUIStyle(EditorStyles.label);

            var texNormal = new Texture2D(1, 1) { alphaIsTransparency = true };
            texNormal.SetPixel(1, 1, transparentColor);
            texNormal.Apply();

            var texHover = new Texture2D(1, 1) { alphaIsTransparency = true };
            texHover.SetPixel(1, 1, highlightColor);
            texHover.Apply();

            style.normal.background = texNormal;
            style.hover.background = texHover;

            return style;
        }

        // Draw the whole collision matrix view.
        public static bool Draw(SerializedObject serializedObject) {
            BetterPhysicsSettings settings = (BetterPhysicsSettings)serializedObject.targetObject;
            const int checkboxSize = 32;
            var labelSize = 110;
            const int indent = 30;

            bool changesMade = false;

            // Find the longest label
            for (var i = 0; i < BetterPhysics.DefinedLayerCount; ++i) {
                var textDimensions = GUI.skin.label.CalcSize(new GUIContent(BetterPhysics.LayerIndexToName(i)));
                if (labelSize < textDimensions.x)
                    labelSize = (int)textDimensions.x;
            }

            GUILayout.BeginScrollView(new Vector2(0, 0), GUILayout.Height(labelSize + 15));
            var topLabelRect = GUILayoutUtility.GetRect(checkboxSize * BetterPhysics.DefinedLayerCount + labelSize,
                labelSize);
            Rect scrollArea = (Rect)topMostRectProp.GetValue(null, null); // GUIClip.topmostRect;
            // Vector2 topLeft = (Vector2)UnclipMethod.Invoke(null, new object[] { new Vector2(topLabelRect.x - 10, topLabelRect.y) }); 
            // Vector2 topLeft = new(topLabelRect.x - 10, topLabelRect.y);
            Vector2 topLeft = (Vector2)UnclipMethod.Invoke(null,
                new object[] {
                    new Vector2(topLabelRect.x - 10, topLabelRect.y)
                }); // GUIClip.Unclip(new Vector2(topLabelRect.x - 10, topLabelRect.y));

            for (var i = 0; i < BetterPhysics.DefinedLayerCount; ++i) {
                var hideLabel = false;
                var hideLabelOnScrollbar = false;
                var defaultLabelRectWidth = 311;
                var defaultLabelCount = 10;
                var clipOffset = labelSize + (checkboxSize * BetterPhysics.DefinedLayerCount) + checkboxSize;

                // Hide vertical labels when they overlap with the rest of the UI.
                if ((topLeft.x + (clipOffset - checkboxSize * i)) <= 0)
                    hideLabel = true;

                // Hide label when it touches the horizontal scroll area.
                if (topLabelRect.height > scrollArea.height) {
                    hideLabelOnScrollbar = true;
                }
                else if (topLabelRect.width != scrollArea.width ||
                         topLabelRect.width != scrollArea.width - topLeft.x) {
                    // Hide label when it touch vertical scroll area.
                    if (topLabelRect.width > defaultLabelRectWidth) {
                        var tmp = topLabelRect.width - scrollArea.width;
                        if (tmp > 1) {
                            if (topLeft.x < 0)
                                tmp += topLeft.x;

                            if (tmp / checkboxSize > i)
                                hideLabelOnScrollbar = true;
                        }
                    }
                    else {
                        var tmp = defaultLabelRectWidth;
                        if (BetterPhysics.DefinedLayerCount < defaultLabelCount) {
                            tmp -= checkboxSize * (defaultLabelCount - BetterPhysics.DefinedLayerCount);
                        }

                        if ((scrollArea.width + i * checkboxSize) + checkboxSize <= tmp)
                            hideLabelOnScrollbar = true;

                        // Reenable the label when we move the scroll bar.
                        if (topLeft.x < 0) {
                            if (topLabelRect.width == scrollArea.width - topLeft.x)
                                hideLabelOnScrollbar = false;

                            if (BetterPhysics.DefinedLayerCount <= defaultLabelCount / 2) {
                                if ((tmp - (scrollArea.width - ((topLeft.x - 10) * (i + 1)))) < 0)
                                    hideLabelOnScrollbar = false;
                            }
                            else {
                                float hiddenlables = (int)(tmp - scrollArea.width) / checkboxSize;
                                int res = (int)((topLeft.x * -1) + 12) / checkboxSize;
                                if (hiddenlables - res < i)
                                    hideLabelOnScrollbar = false;
                            }
                        }
                    }
                }

                var translate =
                    new Vector3(                        
                        labelSize + indent + checkboxSize * (BetterPhysics.DefinedLayerCount - i) + topLeft.y +
                        topLeft.x + 10,
                        topLeft.y, 0);
                GUI.matrix = Matrix4x4.TRS(translate, Quaternion.identity, Vector3.one) *
                             Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 90), Vector3.one);

                var labelRect = new Rect(2 - topLeft.x, 0, labelSize, checkboxSize + 5);
                if (hideLabel || hideLabelOnScrollbar) {
                    GUI.Label(labelRect, GUIContent.none, Styles.rightLabel);
                }
                else {
                    GUI.Label(labelRect, BetterPhysics.LayerIndexToName(i), Styles.rightLabel);

                    // Empty Transparent label used to indicate highlighted row
                    var checkRect = new Rect(2 - topLeft.x, 1 /*This centers the highlight*/,
                        labelSize + 4 + (i + 1) * checkboxSize, checkboxSize);
                    GUI.Label(checkRect, GUIContent.none, Styles.hoverStyle);

                    checkRect = new Rect(
                        GUI.matrix.MultiplyPoint(new Vector3(checkRect.position.x, checkRect.position.y + 200,
                            0)),
                        GUI.matrix.MultiplyPoint(new Vector3(checkRect.size.x, checkRect.size.y, 0)));


                    // GUIView.current.MarkHotRegion(labelRect);
                    // GUIView.current.MarkHotRegion(checkRect);
                    object currentGuiView = guiViewCurrentProp.GetValue(null, null);
                    MarkHotRegionMethod.Invoke(currentGuiView, new object[] { labelRect });
                    MarkHotRegionMethod.Invoke(currentGuiView, new object[] { checkRect });
                }
            }

            GUILayout.EndScrollView();

            {
                GUI.matrix = Matrix4x4.identity;
                for (var i = 0; i < BetterPhysics.DefinedLayerCount; i++) {
                    // Yes so draw toggles.
                    var r = GUILayoutUtility.GetRect(
                        indent + checkboxSize * BetterPhysics.DefinedLayerCount + labelSize,
                        checkboxSize);
                    var labelRect = new Rect(r.x + indent, r.y, labelSize, checkboxSize + 5);
                    GUI.Label(labelRect, BetterPhysics.LayerIndexToName(i), Styles.rightLabel);

                    // Empty Transparent label used to indicate highlighted row.
                    var checkRect = new Rect(r.x + indent, r.y,
                        labelSize + (BetterPhysics.DefinedLayerCount - i) * checkboxSize,
                        checkboxSize);
                    GUI.Label(checkRect, GUIContent.none, Styles.hoverStyle);

                    // GUIView.current.MarkHotRegion(labelRect);
                    // GUIView.current.MarkHotRegion(checkRect);
                    object currentGuiView = guiViewCurrentProp.GetValue(null, null);
                    MarkHotRegionMethod.Invoke(currentGuiView, new object[] { labelRect });
                    MarkHotRegionMethod.Invoke(currentGuiView, new object[] { checkRect });

                    var oldColor = GUI.backgroundColor;
                    // Iterate all the layers.
                    for (var j = BetterPhysics.DefinedLayerCount - 1; j >= 0; j--) {
                        if (j < BetterPhysics.DefinedLayerCount - i) {
                            // TODO disable "reserved" layers (Unstoppable and Feather)
                            int actor = i;
                            int receiver = BetterPhysics.DefinedLayerCount - j - 1;
                            var tooltip = new GUIContent("",
                                BetterPhysics.LayerIndexToName(actor) + "/" + BetterPhysics.LayerIndexToName(receiver));
                            var interactionType = settings.GetInteractionOrDefault(actor, receiver).InteractionType;
                            var thisRect = new Rect(labelSize + indent + r.x + j * checkboxSize, r.y,
                                checkboxSize,
                                checkboxSize);
                            
                            GUI.backgroundColor = GetColor(interactionType);
                            var wasEnabled = GUI.enabled;
                            GUI.enabled = !(InteractionLayer.IsReservedLayer(actor) ||
                                            InteractionLayer.IsReservedLayer(receiver));
                            if (GUI.Button(thisRect, tooltip)) {
                                InteractionType newType =
                                    (InteractionType)(((int)interactionType + 1) % InteractionTypeCount);
                                settings.SetLayerInteraction(actor, receiver, newType);
                                // settings.UpdateLayerInteractionMatrix(new(actor, receiver), newType);
                                changesMade = true;
                            }

                            GUI.enabled = wasEnabled;
                        }
                    }

                    GUI.backgroundColor = oldColor;
                }
                
                serializedObject.Update();
            }

            // Buttons.
            {
                EditorGUILayout.Space(8);
                GUILayout.BeginHorizontal();

                // Made the buttons span the entire matrix of layers
                if (GUILayout.Button("Reset Interactions Matrix", GUILayout.MinWidth(checkboxSize * BetterPhysics.DefinedLayerCount),
                        GUILayout.ExpandWidth(false))) {
                    BetterPhysicsSettings.Instance.ResetAllLayerInteractions();
                    serializedObject.Update();
                    changesMade = true;
                }

                GUILayout.EndHorizontal();
                
                
            }

            return changesMade;
        }

        private static Color GetColor(InteractionType interactionType) {
            switch (interactionType) {
                case InteractionType.Feather:
                    return FeatherInteractionColor;
                case InteractionType.Kinematic:
                    return KinematicInteractionColor;
                default:
                    return DefaultInteractionColor;
            }
        }

        static void SetAllLayerCollisions(InteractionType flag, SetValueFunc setValue) {
            for (int i = 0; i < BetterPhysics.DefinedLayerCount; ++i)
            for (int j = i; j < BetterPhysics.DefinedLayerCount; ++j)
                setValue(i, j, flag);
        }
    }
}