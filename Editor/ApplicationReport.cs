using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace JellyTools.JellySceneResourcesReport
{
    public class ApplicationReport : IReport, IProcessable
    {
        private readonly AtlasToolSettings _settings;

        private readonly ApplicationSpritesData _applicationSpritesData;
        private readonly FoldoutsHelper _foldoutsHelper;

        public bool HasData
        {
            get { return _applicationSpritesData.Data.Count > 0 || _applicationSpritesData.CommonSprites.Count > 0; }
        }

        public ApplicationReport(AtlasToolSettings settings)
        {
            _settings = settings;
            _applicationSpritesData = new ApplicationSpritesData();
            _foldoutsHelper = new FoldoutsHelper();
        }

        public void Draw()
        {
            if (!HasData)
            {
                return;
            }

            if (_applicationSpritesData.Data.Count > 0)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label("REPORT:");
                if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                {
                    _applicationSpritesData.Clear();
                }

                GUILayout.EndHorizontal();

                foreach (var sceneData in _applicationSpritesData.Data)
                {
                    if (sceneData.Sprites.Count == 0) continue;

                    DrawSpritesCollectionGui(sceneData.SceneName, sceneData.Sprites);
                }
            }

            if (_applicationSpritesData.CommonSprites.Count > 0)
            {
                DrawSpritesCollectionGui("Common", _applicationSpritesData.CommonSprites);
            }

            if (GUILayout.Button("Reorganize all sprites"))
            {
                if (EditorUtility.DisplayDialog("Warning!",
                    "Are you sure you want to reorganize all sprites in the project? This action cannot be undone!",
                    "You bet!", "Hell, no!"))
                {
                    AssetDatabase.StartAssetEditing();
                    Process();
                    AssetDatabase.StopAssetEditing();
                }
            }
        }

        public void Build()
        {
            _applicationSpritesData.Clear();
            var scenesInBuild = EditorBuildSettings.scenes;

            var currentScenePath = SceneManager.GetActiveScene().path;
            for (var i = 0; i < scenesInBuild.Length; i++)
            {
                if (!EditorUtility.DisplayCancelableProgressBar("Operation in progress",
                    "Processing scene: " + scenesInBuild[i].path,
                    (float) i / scenesInBuild.Length))
                {
                    var scene = scenesInBuild[i];
                    var sceneData = CollectInScene(scene.path);
                    _applicationSpritesData.AddSceneData(sceneData);
                }
                else
                {
                    EditorUtility.ClearProgressBar();
                    break;
                }
            }

            _applicationSpritesData.ProcessCommonSprites();
            EditorUtility.ClearProgressBar();
            EditorSceneManager.OpenScene(currentScenePath);
        }

        public void Process()
        {
            if (!HasData)
            {
                return;
            }

            foreach (var sceneData in _applicationSpritesData.Data)
            {
                if (sceneData.Sprites.Count > 0)
                {
                    MoveSprites(sceneData.SceneName, sceneData.Sprites);

#if UNITY_2017_1_OR_NEWER
                    CreateAtlas(sceneData.SceneName);
#endif
                }
            }

            CreateCommonAtlas(_applicationSpritesData.CommonSprites);
            AssetDatabase.Refresh();
#if UNITY_2017_1_OR_NEWER
            SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);
#endif
        }

        private void DrawSpritesCollectionGui(string atlasName, IList<Texture2D> sprites)
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            _foldoutsHelper[atlasName] = EditorGUILayout.Foldout(_foldoutsHelper[atlasName],
                atlasName + ", " + sprites.Count + " sprites");
            if (GUILayout.Button("Process this scene", GUILayout.ExpandWidth(false)))
            {
                if (sprites.Count > 0)
                {
                    MoveSprites(atlasName, sprites);

#if UNITY_2017_1_OR_NEWER
                    CreateAtlas(atlasName);
#endif
                }
            }

            if (GUILayout.Button("Apply compression", GUILayout.ExpandWidth(false)))
            {
                foreach (var sprite in sprites)
                {
                    var path = AssetDatabase.GetAssetPath(sprite);
                    TextureUtils.ApplyTextureCompression(path, _settings.Format, _settings.Compression);
                }
            }

            GUILayout.EndHorizontal();

            if (_foldoutsHelper[atlasName])
            {
                foreach (var sprite in sprites)
                {
                    DrawSpriteGui(sprite);
                }
            }
        }

        private static SceneData CollectInScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath);
            var images = Resources.FindObjectsOfTypeAll<Image>();

            var sceneData = new SceneData(scene.name);
            for (var i = 0; i < images.Length; i++)
            {
                var image = images[i];
                if (image.sprite == null)
                {
                    continue;
                }

                var oldPath = AssetDatabase.GetAssetPath(image.sprite);

                if (!IsResource(oldPath))
                {
                    sceneData.AddSprite(image.sprite.texture);
                }
            }

            return sceneData;
        }

        private void MoveSprites(string folderName, IList<Texture2D> sprites)
        {
            var atlasesPath = Path.Combine(_settings.AtlasesPath, folderName);
            if (!Directory.Exists(atlasesPath))
            {
                Directory.CreateDirectory(atlasesPath);
                AssetDatabase.ImportAsset(atlasesPath);
                AssetDatabase.Refresh();
            }

            for (var i = 0; i < sprites.Count; i++)
            {
                var sprite = sprites[i];
                EditorUtility.DisplayProgressBar("Operation in progress",
                    "Moving sprite: " + folderName + "." + sprite.name,
                    (float) i / sprites.Count);
                    var oldPath = AssetDatabase.GetAssetPath(sprite);

                    var fileName = Path.GetFileName(oldPath);
                    var newPath = Path.Combine(atlasesPath, fileName);
                    if (oldPath != newPath)
                    {
                        AssetDatabase.MoveAsset(oldPath, atlasesPath + fileName);
                    }

#if !UNITY_2017_1_OR_NEWER
                    TextureUtils.SetPackingTag(atlasesPath + fileName, folderName);
#endif
            }

            EditorUtility.ClearProgressBar();
        }

        private void CreateCommonAtlas(IList<Texture2D> sprites)
        {
            if (sprites.Count == 0)
            {
                return;
            }

            MoveSprites("Common", sprites);

#if UNITY_2017_1_OR_NEWER
            CreateAtlas("Common");
#endif
        }

        private static void DrawSpriteGui(Texture2D sprite)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                GUILayout.Label("Sprite: " + sprite.name);
                GUILayout.Label("Path: " + AssetDatabase.GetAssetPath(sprite));
                if (GUILayout.Button("Show sprite", GUILayout.MaxWidth(120)))
                {
                    Selection.activeObject = sprite;
                }
            }
            GUILayout.EndVertical();
        }

#if UNITY_2017_1_OR_NEWER
        private void CreateAtlas(string atlasName)
        {
            var folderName = atlasName;

            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(
                Path.Combine(_settings.AtlasesPath, folderName + ".spriteatlas"));
            if (atlas == null)
            {
                AssetDatabase.CopyAsset(Path.Combine(_settings.AtlasesPath, "example.spriteatlas"),
                    Path.Combine(_settings.AtlasesPath, folderName + ".spriteatlas"));
                atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(
                    Path.Combine(_settings.AtlasesPath, folderName + ".spriteatlas"));
            }

            atlas.Add(new[]
            {
                AssetDatabase.LoadAssetAtPath(Path.Combine(_settings.AtlasesPath, folderName), typeof(Object))
            });
        }
#endif

        private static bool IsResource(string path)
        {
            return path.Contains("Resources") || path.Contains("AssetBundles") ||
                   path.Contains("StreamingAssets");
        }
    }
}