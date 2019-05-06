#if UNITY_EDITOR
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
    public class JellyAtlasesOptimizationToolWindow : EditorWindow
    {
        private const string ATLASES_PATH = "Assets/Sprites/UI/Atlases";

        private readonly ApplicationSpritesData _applicationSpritesData;
        private readonly IList<Image> _imagesInAtlas;
        private readonly IList<Image> _imagesNotInAtlas;
        private readonly IDictionary<string, bool> _foldouts;

        private JellyAtlasesOptimizationToolWindow()
        {
            _foldouts = new Dictionary<string, bool>(64);
            _imagesInAtlas = new List<Image>(64);
            _imagesNotInAtlas = new List<Image>(64);
            _applicationSpritesData = new ApplicationSpritesData();
        }

        [MenuItem("Jelly Tools/Atlases optimization tool")]
        public static void Initialize()
        {
            GetWindow<JellyAtlasesOptimizationToolWindow>(true, "Atlases optimization tool");
        }

        private Vector2 _scrollPosition;
        private string _moveToPath;

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Label("IMAGES IN ATLAS:", GUI.skin.box);

            foreach (var image in _imagesInAtlas)
            {
                DrawImageGui(image);
            }

            GUILayout.Label("IMAGES NOT IN ATLAS:", GUI.skin.box);

            foreach (var image in _imagesNotInAtlas)
            {
                DrawImageGui(image);
            }

            GUILayout.Label("REPORT:", GUI.skin.box);

            foreach (var sceneData in _applicationSpritesData.Data)
            {
                _foldouts[sceneData.SceneName] = EditorGUILayout.Foldout(_foldouts[sceneData.SceneName],
                    sceneData.SceneName + ", " + sceneData.Sprites.Count + " sprites");

                if (_foldouts[sceneData.SceneName])
                {
                    foreach (var sprite in sceneData.Sprites)
                    {
                        DrawSpriteGui(sprite);
                    }
                }
            }

            GUILayout.Label("Common: " + _applicationSpritesData.CommonSprites.Count + " sprites", GUI.skin.box);

            foreach (var sprite in _applicationSpritesData.CommonSprites)
            {
                DrawSpriteGui(sprite);
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Build Scene Report"))
            {
                var images = Resources.FindObjectsOfTypeAll<Image>();

                _imagesInAtlas.Clear();
                _imagesNotInAtlas.Clear();
                foreach (var image in images)
                {
                    if (image.sprite != null)
                    {
                        var isInAtlas = image.sprite.packed;
                        if (isInAtlas)
                        {
                            _imagesInAtlas.Add(image);
                        }
                        else
                        {
                            _imagesNotInAtlas.Add(image);
                        }
                    }
                }
            }

            if (GUILayout.Button("Build Project Report"))
            {
                CollectData();
            }

            if (GUILayout.Button("Move all to path"))
            {
                Process();
            }
        }

        private static void DrawImageGui(Image image)
        {
            if (image == null)
            {
                return;
            }

            GUILayout.BeginVertical(GUI.skin.box);
            {
                GUILayout.Label("Sprite: " + image.sprite.name);
                GUILayout.Label("Atlas: " + image.sprite.texture.name);
                GUILayout.Label("Object: " + image.name);
                GUILayout.Label("Path: " + AssetDatabase.GetAssetPath(image.sprite));
                GUILayout.BeginHorizontal(GUI.skin.box);
                {
                    if (GUILayout.Button("Show object", GUILayout.MaxWidth(120)))
                    {
                        Selection.activeObject = image;
                    }

                    if (GUILayout.Button("Show sprite", GUILayout.MaxWidth(120)))
                    {
                        Selection.activeObject = image.sprite;
                    }

                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
        }

        private static void DrawSpriteGui(Sprite sprite)
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

        private void Process()
        {
            if (_applicationSpritesData.Data.Count == 0)
            {
                CollectData();
            }

            foreach (var sceneData in _applicationSpritesData.Data)
            {
                if (sceneData.Sprites.Count > 0)
                {
                    MoveSprites(sceneData.SceneName, sceneData.Sprites);
                    CreateAtlas(sceneData.SceneName);
                }
            }

            CreateCommonAtlas(_applicationSpritesData.CommonSprites);
            AssetDatabase.Refresh();
            SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);
        }

        private void CollectData()
        {
            _foldouts.Clear();
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
                    _foldouts.Add(sceneData.SceneName, false);
                }
            }

            _applicationSpritesData.ProcessCommonSprites();
            EditorUtility.ClearProgressBar();
            EditorSceneManager.OpenScene(currentScenePath);
        }

        private static SceneData CollectInScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath);
            var images = Resources.FindObjectsOfTypeAll<Image>();

            var sceneData = new SceneData(scene.name);
            for (var i = 0;
                i < images.Length;
                i++)
            {
                var image = images[i];
                if (image.sprite == null)
                {
                    continue;
                }

                var oldPath = AssetDatabase.GetAssetPath(image.sprite);

                if (!IsResource(oldPath))
                {
                    sceneData.AddSprite(image.sprite);
                }
            }

            return sceneData;
        }

        private static void CreateAtlas(string atlasName)
        {
            var folderName = atlasName;

            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(
                Path.Combine(ATLASES_PATH, folderName + ".spriteatlas"));
            if (atlas == null)
            {
                AssetDatabase.CopyAsset(Path.Combine(ATLASES_PATH, "example.spriteatlas"),
                    Path.Combine(ATLASES_PATH, folderName + ".spriteatlas"));
                atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(
                    Path.Combine(ATLASES_PATH, folderName + ".spriteatlas"));
            }

            atlas.Add(new[]
            {
                AssetDatabase.LoadAssetAtPath(Path.Combine(ATLASES_PATH, folderName), typeof(Object))
            });
        }

        private static void MoveSprites(string folderName, IList<Sprite> sprites)
        {
            var newPath = ATLASES_PATH + "/" + folderName + "/";
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
                AssetDatabase.ImportAsset(newPath);
                AssetDatabase.Refresh();
            }

            for (var i = 0; i < sprites.Count; i++)
            {
                var sprite = sprites[i];
                if (!EditorUtility.DisplayCancelableProgressBar("Operation in progress",
                    "Moving sprite: " + sprite.name,
                    (float) i / sprites.Count))
                {
                    var oldPath = AssetDatabase.GetAssetPath(sprite);

                    var fileName = Path.GetFileName(oldPath);
                    if (oldPath != newPath + fileName)
                    {
                        AssetDatabase.MoveAsset(oldPath, newPath + fileName);
                    }
                }
            }

            EditorUtility.ClearProgressBar();
        }

        private static void CreateCommonAtlas(IList<Sprite> sprites)
        {
            if (sprites.Count == 0)
            {
                return;
            }

            MoveSprites("Common", sprites);
            CreateAtlas("Common");
            AssetDatabase.Refresh();
        }

        private static bool IsResource(string path)
        {
            return path.Contains("Resources") || path.Contains("AssetBundles") ||
                   path.Contains("StreamingAssets");
        }
    }
}

#endif