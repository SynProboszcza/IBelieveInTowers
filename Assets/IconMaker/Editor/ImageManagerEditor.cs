using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Linq;
using System;

namespace Melon
{
    [CustomEditor(typeof(ImageManager))]
    [CanEditMultipleObjects]
    public class ImageManagerEditor : ResolutionManager
    {
        public override bool RequiresConstantRepaint() => true;

        public ImageManager renderer;

        public int windowNum;

        private SerializedProperty renderCamera;
        private SerializedProperty targetObject;
        private SerializedProperty path;
        private SerializedProperty width;
        private SerializedProperty height;
        private SerializedProperty resolution;
        private SerializedProperty resolutionOption;
        private SerializedProperty customResolution;
        private SerializedProperty transparent;
        private SerializedProperty backgroundColor;
        private SerializedProperty op;

        public string[] resolutionOptions = new string[] { "256x256", "512x512", "1024x1024", "2048x2048", "4096x4096" };

        [MenuItem("Tools/Icon Maker/Create an Instance", priority = 1)]
        public static void CreateIconMaker()
        {
            GameObject iconMaker = Instantiate(Resources.Load("IconMaker") as GameObject);
            iconMaker.name = "Icon Maker";
            ImageManager.Instance = iconMaker.GetComponent<ImageManager>();
            Debug.Log("Creating instance of Icon Maker");
        }

        void OnEnable()
        {
            serializedObject.Update();

            //Loading all textures
            serializedObject.FindProperty("icon1").objectReferenceValue = Resources.Load<Texture>("Resolution Manager Icon");
            serializedObject.FindProperty("icon2").objectReferenceValue = Resources.Load<Texture>("Resolution Manager Icon");
            serializedObject.FindProperty("icon3").objectReferenceValue = Resources.Load<Texture>("Settings Icon");

            renderCamera = serializedObject.FindProperty("renderCamera");
            targetObject = serializedObject.FindProperty("target");
            path = serializedObject.FindProperty("path");
            width = serializedObject.FindProperty("width");
            height = serializedObject.FindProperty("height");
            resolution = serializedObject.FindProperty("resolution");
            resolutionOption = serializedObject.FindProperty("_resolutionOption");
            customResolution = serializedObject.FindProperty("_customResolution");
            transparent = serializedObject.FindProperty("_transparent");
            backgroundColor = serializedObject.FindProperty("_backgroundColor");
            op = serializedObject.FindProperty("_op");

            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            renderer = (ImageManager)target;

            List<GUIContent> icons = new List<GUIContent>();
            Rect position = EditorGUILayout.GetControlRect(GUILayout.Height(0));

            if (Screen.width > 450)
            {
                icons.Add(new GUIContent("    General", (Texture)serializedObject.FindProperty("icon1").objectReferenceValue, "Manage how to create your texture."));
                //icons.Add(new GUIContent("    Lighting", (Texture)serializedObject.FindProperty("icon2").objectReferenceValue, "Setup Lights, shadows, intensity and more"));
                icons.Add(new GUIContent("    Settings", (Texture)serializedObject.FindProperty("icon3").objectReferenceValue, "Change export settings and more"));
            }
            else
            {
                icons.Add(new GUIContent((Texture)serializedObject.FindProperty("icon1").objectReferenceValue, "Manage how to create your texture."));
                //icons.Add(new GUIContent((Texture)serializedObject.FindProperty("icon2").objectReferenceValue, "Setup Lights, shadows, intensity and more"));
                icons.Add(new GUIContent((Texture)serializedObject.FindProperty("icon3").objectReferenceValue, "Change export settings and more"));
            }

            GUIStyle iconStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
            iconStyle.fixedHeight = 40;
            iconStyle.fixedWidth = (position.width / 2) - 5;
            iconStyle.margin = new RectOffset(5, 5, 5, 5);
            iconStyle.padding = new RectOffset(5, 5, 5, 5);
            iconStyle.fontStyle = FontStyle.Bold;

            windowNum = serializedObject.FindProperty("window").intValue;
            int j = GUILayout.SelectionGrid(windowNum, icons.ToArray(), 4, iconStyle);

            if (j != windowNum)
            {

                windowNum = j;

            }

            serializedObject.FindProperty("window").intValue = windowNum;

            switch (windowNum)
            {
                case (0):
                    General();
                    break;
                case (1):
                    Settings();
                    break;
                case (2):
                    Lighting();
                    break;
                default:
                    General();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        #region Tabs

        #region General

        public void General()
        {
            //Styles
            GUIStyle regularButtonStyle = new GUIStyle("Button");

            GUIStyle actionButtonStyle = new GUIStyle("LargeButtonMid");
            actionButtonStyle.fontStyle = FontStyle.Bold;
            actionButtonStyle.fontSize = 18;
            actionButtonStyle.normal.textColor = Color.red;

            //Texts
            GUIStyle boldText = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };

            EditorGUILayout.LabelField("Icon Preview");

            EditorGUILayout.Space();

            if (GUILayout.Button("Show Icon Preview", regularButtonStyle))
            {
                AddResolution(renderer.width, renderer.height, "Icon Preview");
                SetResolution(GetCount() - 1);
            }
            if (GUILayout.Button("Reset to default", regularButtonStyle))
            {
                SetResolution(0);
                RemoveResolution(GetCount() - 1);
            }

            EditorGUILayout.Space();

            if (customResolution.boolValue)
                EditorGUILayout.LabelField($"Current size : {width.intValue} x {height.intValue} ", boldText);
            else
                EditorGUILayout.LabelField($"Current size : {resolution.intValue * 256} x {resolution.intValue * 256} ", boldText);

            customResolution.boolValue = EditorGUILayout.Toggle("Custom Resolution ? ", customResolution.boolValue);

            if (!customResolution.boolValue)
            {
                resolutionOption.intValue = EditorGUILayout.Popup("Select Resolution", resolutionOption.intValue, resolutionOptions, EditorStyles.popup);

                switch (resolutionOption.intValue)
                {
                    case 0:
                        resolution.intValue = 1;
                        renderer.width = 256;
                        renderer.height = 256;
                        break;
                    case 1:
                        resolution.intValue = 2;
                        renderer.width = 512;
                        renderer.height = 512;
                        break;
                    case 2:
                        resolution.intValue = 4;
                        renderer.width = 1024;
                        renderer.height = 1024;
                        break;
                    case 3:
                        resolution.intValue = 8;
                        renderer.width = 2048;
                        renderer.height = 2048;
                        break;
                    case 4:
                        resolution.intValue = 16;
                        renderer.width = 4096;
                        renderer.height = 4096;
                        break;
                }
            }

            if (customResolution.boolValue)
            {
                GUIStyle intField = new GUIStyle("AnimationSelectionTextField");
                intField.alignment = TextAnchor.MiddleLeft;
                intField.stretchWidth = true;

                EditorGUILayout.BeginHorizontal();

                EditorGUIUtility.labelWidth = 1;

                EditorGUILayout.LabelField("Custom Resolution");
                width.intValue = EditorGUILayout.IntField(width.intValue, intField);
                height.intValue = EditorGUILayout.IntField(height.intValue, intField);
                EditorGUILayout.EndHorizontal();

                EditorGUIUtility.labelWidth = 0;
            }

            op.intValue = EditorGUILayout.Popup("Select Format", op.intValue, op.enumDisplayNames, EditorStyles.popup);
            renderer.fileExtension = op.intValue;

            if (op.intValue == 0)
            {
                transparent.boolValue = EditorGUILayout.Toggle("Transparent Icon ?", transparent.boolValue);
                renderer.transparent = transparent.boolValue;

                if (!transparent.boolValue)
                {
                    backgroundColor.colorValue = EditorGUILayout.ColorField("Background Color", backgroundColor.colorValue);
                    renderer.backgroundColor = backgroundColor.colorValue;
                }
            }
            else
            {
                backgroundColor.colorValue = EditorGUILayout.ColorField("Background Color", backgroundColor.colorValue);
                renderer.backgroundColor = backgroundColor.colorValue;
            }

            if (GUILayout.Button("Render Icon", actionButtonStyle))
            {
                renderer.SaveImageAsFile();
            }

            EditorGUILayout.Space();

            #region References Foldout

            GUIStyle labelStyle = new GUIStyle("ButtonMid");
            labelStyle.fontStyle = FontStyle.Bold;

            serializedObject.FindProperty("referencesFoldout").boolValue
                = EditorGUILayout.BeginFoldoutHeaderGroup(serializedObject.FindProperty("referencesFoldout").boolValue, new GUIContent("References"), labelStyle);

            if (serializedObject.FindProperty("referencesFoldout").boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(renderCamera, new GUIContent("Render Camera"));
                EditorGUILayout.PropertyField(targetObject, new GUIContent("Target to render"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            #endregion
        }

        #endregion

        #region Lighting

        public void Lighting()
        {
            //TODO : Move Lighting manager here
        }

        #endregion

        #region Settings

        public void Settings()
        {
            #region Settings foldout

            EditorGUILayout.PropertyField(path, new GUIContent("Path to save files"));

            EditorGUILayout.Space();

            if (GUILayout.Button("Choose a path", EditorStyles.miniButtonMid))
            {
                PathPopup.ShowWindow();
            }
            renderer.path = PlayerPrefs.GetString("path");

            #endregion
        }

        #endregion

        #endregion
    }
}