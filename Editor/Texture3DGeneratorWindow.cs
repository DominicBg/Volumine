using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.Mathematics;

namespace Volumine
{
    [CustomEditor(typeof(VolumineNoiseGenerator))]
    public class VolumineNoiseGeneratorWindow : Editor
    {
        const string shaderName = "Volumine/VolumetricTextureVisualiser";
        public string textureName = "Cloud";
        public string path = "Assets/Art/Generated/";
        public float rotationYSlider = 45f;
        public float rotationXSlider = 45f;
        public float zoomSlider = 4f;

        public float alphaSlider = 0.5f;
        private Material renderMaterial;
        private int maxSize3D = 32;

        Texture2D preview2D;
        Texture3D preview3D;

        private void OnEnable()
        {
            renderMaterial = new Material(Shader.Find(shaderName));
            var data = (VolumineNoiseGenerator)target;
            data.OnValidateCallback.AddListener(RecalculatePreview);

            RecalculatePreview();
        }
        private void OnDisable()
        {
            var data = (VolumineNoiseGenerator)target;
            data.OnValidateCallback.RemoveListener(RecalculatePreview);

        }
        private void RecalculatePreview()
        {
            var data = (VolumineNoiseGenerator)target;
            preview2D = VolumineAPI.GenerateTexture2D(data.sizes.xy, data.noises);
            preview3D = VolumineAPI.GenerateTexture3D(math.min(data.sizes, maxSize3D), data.noises);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var data = (VolumineNoiseGenerator)target;
            using (new EditorGUILayout.VerticalScope())
            {
                path = EditorGUILayout.TextField("Path", path);
                textureName = EditorGUILayout.TextField("Filename", textureName);
            }

            if (GUILayout.Button("Generate Texture 2D"))
            {
                SaveTextureAsPNG(VolumineAPI.GenerateTexture2D(data.sizes.xy, data.noises));
            }
            if (GUILayout.Button("Generate Texture 3D"))
            {
                SaveTextureAsAsset(VolumineAPI.GenerateTexture3D(data.sizes, data.noises));
            }
            if (GUILayout.Button("Generate Texture 32x32x32"))
            {
                SaveTextureAsPNG(VolumineAPI.GenerateTexture32x32x32(data.noises));
            }

            ShowSliders();

            SetTransformInShader();

            renderMaterial.SetTexture("_VolumeTexture", preview3D);
            renderMaterial.SetFloat("alpha", alphaSlider);

            const int textureSize = 256;
            var controlRect = EditorGUILayout.GetControlRect();
            EditorGUI.DrawPreviewTexture(new Rect(controlRect.x, controlRect.y, textureSize, textureSize), preview2D);
            EditorGUI.DrawPreviewTexture(new Rect(controlRect.x + textureSize * 1.5f, controlRect.y, textureSize, textureSize), Texture2D.whiteTexture, renderMaterial, ScaleMode.ScaleToFit);
        }

        private void ShowSliders()
        {
            rotationYSlider = EditorGUILayout.Slider("Y Rotation", rotationYSlider, 0, 360);
            rotationXSlider = EditorGUILayout.Slider("X Rotation", rotationXSlider, -45, 45);
            zoomSlider = EditorGUILayout.Slider("Zoom", zoomSlider, 1, 5);
            alphaSlider = EditorGUILayout.Slider("Alpha", alphaSlider, 0, 2);
        }

        void SetTransformInShader()
        {
            float3 boxPos = 0;
            math.sincos(math.radians(rotationYSlider), out float z, out float x);
            float y = math.sin(math.radians(rotationXSlider));
            float3 camPos = math.normalize(new float3(x, y, z)) * zoomSlider;

            float3 cameraForward = math.normalize(boxPos - camPos);
            quaternion camRot = quaternion.LookRotation(cameraForward, math.up());

            renderMaterial.SetVector(nameof(camPos), new float4(camPos, 0));
            renderMaterial.SetVector(nameof(camRot), camRot.value);
        }

        void SaveTextureAsPNG(Texture2D texture)
        {
            EnsureDirectory(path);

            byte[] bytes = texture.EncodeToJPG();
            string finalPath = path + textureName + ".jpg";
            File.WriteAllBytes(finalPath, bytes);
            Debug.Log(textureName + " is saved");
            AssetDatabase.Refresh();

            var textureObject = AssetDatabase.LoadAssetAtPath<Texture2D>(finalPath);
            EditorGUIUtility.PingObject(textureObject);
        }

        void SaveTextureAsAsset(Texture3D texture)
        {
            EnsureDirectory(path);

            AssetDatabase.CreateAsset(texture, path + textureName + ".asset");
            Debug.Log(textureName + " is saved");
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(texture);
        }

        void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}