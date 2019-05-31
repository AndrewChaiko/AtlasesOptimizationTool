using System.Collections.Generic;
using UnityEngine;

namespace JellyTools.JellySceneResourcesReport
{
    public class SceneData
    {
        private readonly IList<Texture2D> _sprites;
        private readonly string _sceneName;

        public SceneData(string sceneName)
        {
            _sceneName = sceneName;
            _sprites = new List<Texture2D>();
        }

        public IList<Texture2D> Sprites
        {
            get { return _sprites; }
        }

        public string SceneName
        {
            get { return _sceneName; }
        }

        public void AddSprite(Texture2D sprite)
        {
            if (!HasSprite(sprite))
            {
                _sprites.Add(sprite);
            }
        }

        public bool HasSprite(Texture2D sprite)
        {
            return _sprites.Contains(sprite);
        }
    }
}