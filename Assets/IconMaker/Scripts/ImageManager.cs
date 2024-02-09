using System;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Melon
{
    public enum OPTIONS
    {
        PNG = 0,
        JPG = 1,
        EXR = 2,
        TGA = 3
    }

    [SerializeField]
    [ExecuteInEditMode]
    public class ImageManager : MonoBehaviour
    {
        public static ImageManager Instance { get; set; }

        public void Awake()
        {
            if (Instance != null && Instance != this)
                DestroyImmediate(this);
            else
                Instance = this;

            if (renderCamera == null)
                renderCamera = GetComponentInChildren<Camera>();
        }

        [Header("Components")]
        [Tooltip("Main renderer camera")]
        public Camera renderCamera;
        [Tooltip("Target to render")]
        public GameObject target;

        [Header("Settings")]
        public int fileExtension;
        public string path;
        public bool preview;
        public bool transparent;
        public int width = 1;
        public int height = 1;
        public int resolution = 1;

        public int _resolutionOption;
        public bool _customResolution;
        public bool _transparent = true;
        public Color _backgroundColor;

        public OPTIONS _op;

        public Color backgroundColor;

        [SerializeField]
        private int window;

        public Texture icon1;
        public Texture icon2;
        public Texture icon3;

        public bool referencesFoldout;
        public bool settingsFoldout;

        public void Start()
        {
            CameraDefaultSettings();

            path = Application.dataPath + "/SavedIcons/";
        }

        public void CameraDefaultSettings()
        {
            renderCamera.clearFlags = CameraClearFlags.Color;
            renderCamera.backgroundColor = Color.clear;
            renderCamera.fieldOfView = 60;
            renderCamera.nearClipPlane = 0.01f;
            renderCamera.farClipPlane = 1000;
            renderCamera.orthographic = false;
            renderCamera.usePhysicalProperties = false;
            renderCamera.depth = -1;
        }

        public Texture2D CaptureImage(bool hdr)
        {
            if (renderCamera != null)
            {
                if (target != null)
                {
                    Rect rect = new Rect(0, 0, width, height);
                    RenderTexture renderTexture = new RenderTexture(width, height, 32);
                    Texture2D screenShot = new Texture2D(width, height, hdr ? TextureFormat.RGBAHalf : TextureFormat.ARGB32, false);

                    CameraClearFlags clearFlags = renderCamera.clearFlags;

                    if (transparent)
                    {
                        renderCamera.clearFlags = CameraClearFlags.SolidColor;
                        renderCamera.backgroundColor = Color.clear;
                    }
                    else
                    {
                        renderCamera.clearFlags = CameraClearFlags.SolidColor;
                        renderCamera.backgroundColor = backgroundColor;
                    }

                    renderCamera.targetTexture = renderTexture;
                    renderCamera.Render();

                    RenderTexture.active = renderTexture;
                    screenShot.ReadPixels(rect, 0, 0);
                    screenShot.Apply();

                    renderCamera.targetTexture = null;
                    RenderTexture.active = null;
                    DestroyImmediate(renderTexture);
                    renderCamera.clearFlags = clearFlags;
                    renderTexture = null;
                    return screenShot;
                }
                else
                {
                    Debug.LogError("Target object is empty, please attach 'Target' variable in the Inspector");
                    return null;
                }
            }
            else
            {
                Debug.LogError("Render camera is empty, please attach 'Render Camera' variable in the Inspector");
                return null;
            }
        }

        public void SaveImageAsFile()
        {
            try
            {
                if (width <= 0 || height <= 0)
                {
                    Debug.LogError("The resolution is wrong ! Check your settings and try again."); 
                    return;
                }

                Texture2D texture = fileExtension == 2 ? CaptureImage(true) : CaptureImage(false);

                if (texture != null)
                {
                    //then Save To Disk as PNG
                    byte[] bytes = null;
                    if (fileExtension == 0)
                        bytes = texture.EncodeToPNG();
                    else if (fileExtension == 1)
                        bytes = texture.EncodeToJPG();
                    else if (fileExtension == 2)
                        bytes = texture.EncodeToEXR();
                    else if (fileExtension == 3)
                        bytes = texture.EncodeToTGA();

                    if (path != null)
                    {
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("The destination path is empty ! Please set up destination path in the 'Icon Maker' inspector view or using toolbar");
                    }

                    string extension = ".png";

                    switch (fileExtension)
                    {
                        case 0:
                            extension = ".png";
                            break;
                        case 1:
                            extension = ".jpg";
                            break;
                        case 2:
                            extension = ".exr";
                            break;
                        case 3:
                            extension = ".tga";
                            break;
                        default: 
                            extension = ".png";
                            break;
                    }

                    string name = "Icon_" + System.DateTime.Now.Hour.ToString() + System.DateTime.Now.Minute + System.DateTime.Now.Second + extension;

                    File.WriteAllBytes(path + name, bytes);

                    Debug.Log($"Success ! \nThe icon '{name}' is saved in " + path);
                }
                else
                {
                    Debug.LogError("There is problem with rendering texture from the camera", texture);
                }
            }
            catch (Exception errorMessage)
            {
                Debug.LogException(errorMessage);
            }
        }
    }
}