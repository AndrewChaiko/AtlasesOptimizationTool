using System.Collections.Generic;

namespace JellyTools.JellySceneResourcesReport
{
    public class FoldoutsHelper
    {
        private readonly Dictionary<string, bool> _pool;

        public FoldoutsHelper()
        {
            _pool = new Dictionary<string, bool>(64);
        }

        public bool this[string name]
        {
            get { return GetFoldout(name); }
            set { SetFoldout(name, value); }
        }

        private bool GetFoldout(string name)
        {
            if (_pool.ContainsKey(name))
            {
                return _pool[name];
            }
            else
            {
                _pool.Add(name, false);
            }

            return false;
        }

        private void SetFoldout(string name, bool value)
        {
            _pool[name] = value;
        }
    }
}