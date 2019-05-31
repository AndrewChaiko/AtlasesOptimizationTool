using System.Collections.Generic;
using UnityEngine;

namespace JellyTools.JellySceneResourcesReport
{
    public class ApplicationSpritesData
    {
        private readonly IList<SceneData> _sceneData;
        private readonly IList<Texture2D> _commonSprites;

        public IList<SceneData> Data
        {
            get { return _sceneData; }
        }

        public IList<Texture2D> CommonSprites
        {
            get { return _commonSprites; }
        }

        public ApplicationSpritesData()
        {
            _sceneData = new List<SceneData>(16);
            _commonSprites = new List<Texture2D>(64);
        }

        public void AddSceneData(SceneData sceneData)
        {
            _sceneData.Add(sceneData);
        }

        public void Clear()
        {
            _sceneData.Clear();
            _commonSprites.Clear();
        }

        public void ProcessCommonSprites()
        {
            foreach (var sceneData in _sceneData)
            {
                foreach (var sprite in sceneData.Sprites)
                {                    
                    foreach (var otherSceneData in _sceneData)
                    {                        
                        if (sceneData.SceneName != otherSceneData.SceneName && otherSceneData.HasSprite(sprite))
                        {
                            if (!_commonSprites.Contains(sprite))
                            {
                                _commonSprites.Add(sprite);
                            }
                        }
                    }
                }
            }

            foreach (var commonSprite in _commonSprites)
            {
                foreach (var sceneData in _sceneData)
                {
                    sceneData.Sprites.Remove(commonSprite);
                }
            }
        }
    }
}