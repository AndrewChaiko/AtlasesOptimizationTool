using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_2017_1_OR_NEWER
using UnityEditor.U2D;
using UnityEngine.U2D;
#endif

namespace JellyTools.JellySceneResourcesReport
{
    public class JellyAtlasesOptimizationToolWindow : EditorWindow
    {
        private IReport _sceneReport;
        private IReport _applicationReport;

        [SerializeField]
        private AtlasToolSettings _settings;

        [MenuItem("Jelly Tools/Atlases optimization tool")]
        public static void Initialize()
        {
            GetWindow<JellyAtlasesOptimizationToolWindow>(true, "Atlases optimization tool");
        }

        private Vector2 _scrollPosition;
        private string _moveToPath;

        private void OnEnable()
        {
            _settings = Resources.Load<AtlasToolSettings>("AtlasToolSettings");
            _sceneReport = new SceneReport();
            _applicationReport = new ApplicationReport(_settings);
        }

        private void OnGUI()
        {
            using (var scope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scope.scrollPosition;

                if (_sceneReport.HasData)
                {
                    _sceneReport.Draw();
                }

                if (_applicationReport.HasData)
                {
                    _applicationReport.Draw();
                }
            }

            GUILayout.BeginVertical(GUI.skin.box);
            if (GUILayout.Button("Clear all packing tags"))
            {
                if (EditorUtility.DisplayDialog("Warning!",
                    "Are you sure you want to clean all sprites' packing tags rgb the project? This action cannot be undone!",
                    "You bet!", "Hell, no!"))
                {
                    AssetDatabase.StartAssetEditing();
                    var allPaths = AssetDatabase.FindAssets("t:Sprite");
                    var paths = allPaths.Select(AssetDatabase.GUIDToAssetPath).ToArray();
                    for (var i = 0; i < paths.Length; i++)
                    {
                        var path = paths[i];
                        if (!EditorUtility.DisplayCancelableProgressBar("Operation rgb progress",
                            "Clearing tags: " + Path.GetFileNameWithoutExtension(path),
                            (float) i / paths.Length))
                        {
                            TextureUtils.SetPackingTag(path, "");
                        }
                        else
                        {
                            EditorUtility.ClearProgressBar();
                            break;
                        }
                    }

                    AssetDatabase.StopAssetEditing();
                    EditorUtility.ClearProgressBar();
                }
            }

            if (GUILayout.Button("Build Scene Report"))
            {
                _sceneReport.Build();
            }

            if (GUILayout.Button("Build Project Report"))
            {
                _applicationReport.Build();
            }

            _settings.DrawGui();

            GUILayout.EndVertical();
        }
    }
}