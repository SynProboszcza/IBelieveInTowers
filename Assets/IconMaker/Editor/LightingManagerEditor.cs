using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Melon
{
    [CanEditMultipleObjects]
    public class LightingManagerEditor : Editor
    {
        private SerializedProperty camera;
        private SerializedProperty light;
        private SerializedProperty power;
        private SerializedProperty range;
        private SerializedProperty distance;
        private SerializedProperty angle;

        public Color _lightColor = Color.white;

        void OnEnable()
        {
            camera = serializedObject.FindProperty("_camera");
            light = serializedObject.FindProperty("_light");
            power = serializedObject.FindProperty("intensity");
            range = serializedObject.FindProperty("range");
            distance = serializedObject.FindProperty("distance");
            angle = serializedObject.FindProperty("angle");
        }

        public override void OnInspectorGUI()
        {
            LightingManager lighting = (LightingManager)target;

            EditorGUILayout.LabelField("Main components");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(camera, new GUIContent("Icon camera"));
            EditorGUILayout.PropertyField(light, new GUIContent("Light source"));
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Settings");
            EditorGUI.indentLevel++;
            EditorGUILayout.Slider(power, 0, 10, "Light intensity");
            EditorGUILayout.Slider(range, 0, 100, "Light range");
            EditorGUILayout.Slider(distance, 0, 20, "Light distance from target");
            EditorGUILayout.Slider(angle, 0, 180, "Light angle");
            _lightColor = EditorGUILayout.ColorField("Light Color", _lightColor);
            lighting.lightColor = _lightColor;
            EditorGUI.indentLevel--;

            EditorGUILayout.HelpBox("Values are updated in realtime", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        public void OnGUI()
        {
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
}