using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace JellyTools.JellySceneResourcesReport
{
    public class SceneReport : IReport
    {
        private readonly IList<Image> _imagesInAtlas;
        private readonly IList<Image> _imagesNotInAtlas;
        private readonly FoldoutsHelper _foldoutsHelper;

        public SceneReport()
        {
            _foldoutsHelper = new FoldoutsHelper();
            _imagesInAtlas = new List<Image>(64);
            _imagesNotInAtlas = new List<Image>(64);
        }

        public bool HasData
        {
            get { return _imagesInAtlas.Count > 0 || _imagesNotInAtlas.Count > 0; }
        }

        public void Draw()
        {
            if (_imagesInAtlas.Count > 0)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                _foldoutsHelper["iia"] = EditorGUILayout.Foldout(_foldoutsHelper["iia"],
                    "IMAGES IN ATLAS: " + _imagesInAtlas.Count);
                if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                {
                    _imagesInAtlas.Clear();
                }

                GUILayout.EndHorizontal();

                if (_foldoutsHelper["iia"])
                {
                    foreach (var image in _imagesInAtlas)
                    {
                        DrawImageGui(image);
                    }
                }
            }

            if (_imagesNotInAtlas.Count > 0)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                _foldoutsHelper["inia"] = EditorGUILayout.Foldout(_foldoutsHelper["inia"],
                    "IMAGES NOT IN ATLAS: " + _imagesNotInAtlas.Count);
                if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                {
                    _imagesNotInAtlas.Clear();
                }

                GUILayout.EndHorizontal();

                if (_foldoutsHelper["inia"])
                {
                    foreach (var image in _imagesNotInAtlas)
                    {
                        DrawImageGui(image);
                    }
                }
            }
        }

        public void Build()
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

        private static void DrawImageGui(Image image)
        {
            if (image == null)
            {
                return;
            }

            using (var scope = new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                GUILayout.Label("Sprite: " + image.sprite.name);
                GUILayout.Label("Atlas: " + image.sprite.texture.name);
                GUILayout.Label("Object: " + image.name);
                GUILayout.Label("Path: " + AssetDatabase.GetAssetPath(image.sprite));
                if (GUILayout.Button("Show object", GUILayout.ExpandWidth(false)))
                {
                    Selection.activeObject = image;
                }

                if (GUILayout.Button("Show sprite", GUILayout.ExpandWidth(false)))
                {
                    Selection.activeObject = image.sprite.texture;
                }
            }
        }
    }
}