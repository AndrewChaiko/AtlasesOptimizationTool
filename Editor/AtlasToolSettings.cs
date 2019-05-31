using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JellyTools.JellySceneResourcesReport
{
    [CreateAssetMenu(menuName = "Create AtlasToolSettings", fileName = "AtlasToolSettings.asset")]
    public class AtlasToolSettings : ScriptableObject
    {
        [SerializeField]
        private TextureImporterFormat _format = TextureImporterFormat.Automatic;

        [SerializeField]
        private TextureImporterCompression _compression = TextureImporterCompression.Uncompressed;

        [SerializeField]
        private string _atlasesPath = "Assets/Sprites/UI/Atlases";

        public TextureImporterFormat Format
        {
            get { return _format; }
            set { _format = value; }
        }

        public TextureImporterCompression Compression
        {
            get { return _compression; }
            set { _compression = value; }
        }

        public string AtlasesPath
        {
            get { return _atlasesPath; }
            set { _atlasesPath = value; }
        }

        public void DrawGui()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField("Path to store atlases", _atlasesPath);
                if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
                {
                    _atlasesPath = EditorUtility.OpenFolderPanel("Select folder", _atlasesPath, "");
                    
                    _atlasesPath = _atlasesPath.Substring(_atlasesPath.IndexOf("Assets", StringComparison.Ordinal));
                }
            }

            _format = (TextureImporterFormat) EditorGUILayout.EnumPopup("Texture format", _format);
            _compression = (TextureImporterCompression) EditorGUILayout.EnumPopup("Texture compression", _compression);
            if (GUILayout.Button("Apply compression to ALL textures in project"))
            {
                if (EditorUtility.DisplayDialog("Warning!",
                    "Are you sure you want to change all textures compression settings? This action cannot be undone!",
                    "You bet!", "Hell, no!"))
                {

                    var guids = AssetDatabase.FindAssets("t:Sprite");
                    var paths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
                    for (var i = 0; i < paths.Length; i++)
                    {
                        var path = paths[i];
                        if (!EditorUtility.DisplayCancelableProgressBar("Operation in progress",
                            "Applying settings: " + Path.GetFileNameWithoutExtension(path),
                            (float) i / paths.Length))
                        {
                            TextureUtils.ApplyTextureCompression(path, Format, _compression);
                        }
                        else
                        {
                            EditorUtility.ClearProgressBar();
                            break;
                        }
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    EditorUtility.ClearProgressBar();
                }
            }

            GUILayout.EndVertical();
        }
    }
}