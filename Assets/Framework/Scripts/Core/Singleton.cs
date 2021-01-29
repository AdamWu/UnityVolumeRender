using UnityEngine;
using System.Collections;

namespace Framework
{
    /// <summary>
    /// Singleton base class
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {

        private static T _instance;
        private static object _lock = new object();

        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (applicationIsQuitting) return null;

                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));
                        if (_instance == null)
                        {
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                            singleton.name = string.Format("[{0}]", typeof(T).ToString());
                            DontDestroyOnLoad(singleton);
                        }
                    }
                    return _instance;
                }
            }
        }

        private static bool ClearMode = false;
        private static bool applicationIsQuitting = false;
        /// <summary>
        /// Bugfix When Unity Quit
        /// </summary>
        public void OnDestroy()
        {
            if (ClearMode == false)
                applicationIsQuitting = true;
            ClearMode = false;
        }

        public static void DestorySingleton()
        {
            lock (_lock)
            {
                if (_instance != null)
                {
                    ClearMode = true;
                    GameObject.Destroy(_instance.gameObject);
                    _instance = null;
                }
            }
        }
    }
}
