using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Melon
{
    [ExecuteInEditMode]
    public class LightingManager : MonoBehaviour
    {
        public static LightingManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
                DestroyImmediate(this);
            else
                Instance = this;
        }

        public void Start()
        {
            _camera = ImageManager.Instance.renderCamera;

            if (GetComponent<Light>() != null)
                _light = GetComponent<Light>();
            else if (GetComponentInChildren<Light>() != null)
                _light = GetComponentInChildren<Light>();
        }

        [Header("Components")]
        public Camera _camera;
        public Light _light;
        [Header("Settings")]
        public float intensity = 1;
        public float range = 50;
        public float distance = 0;
        public float angle = 45;
        public Color lightColor;

        public void OnRenderObject()
        {
            _light.intensity = intensity;
            _light.range = range;
            _light.transform.position = _camera.transform.position -  new Vector3(0, 0, distance);
            _light.spotAngle = angle;
            _light.color = lightColor;
        }
    }
}