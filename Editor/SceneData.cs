using System.Collections.Generic;
using UnityEngine;

namespace JellyTools.JellySceneResourcesReport
{
    public class SceneData
    {
        private readonly IList<Sprite> _sprites;
        private readonly string _sceneName;

        public SceneData(string sceneName)
        {
            _sceneName = sceneName;
            _sprites = new List<Sprite>();
        }

        public IList<Sprite> Sprites
        {
            get { return _sprites; }
        }

        public string SceneName
        {
            get { return _sceneName; }
        }

        public void AddSprite(Sprite sprite)
        {
            if (!HasSprite(sprite))
            {
                _sprites.Add(sprite);
            }
        }

        public bool HasSprite(Sprite sprite)
        {
            return _sprites.Contains(sprite);
        }
    }
}