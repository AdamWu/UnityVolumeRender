using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

namespace Framework
{
    public delegate void LoadAssetBundleAsyncDelegate(AssetBundle ab);

    public delegate void HandleDownloadFinish(UnityWebRequest www);
    public delegate void HandleDownloadCallback(string assetbundleName);

    public class AssetBundleManager : Singleton<AssetBundleManager>
    {

        private class LoadedAssetBundle
        {
            public AssetBundle assetBundle;
            public int refCount;
            public LoadedAssetBundle(AssetBundle assetBundle)
            {
                this.assetBundle = assetBundle;
                refCount = 1;
            }
			public void Unload(bool unloadAll)
            {
				assetBundle.Unload(unloadAll);
            }
        }

        void Awake()
        {
            // download path for platforms
            s_BaseDownloadingURL += AssetBundleLoader.GetPlatformFolderForAssetBundles();
        }


        // assetbundles
        static Dictionary<string, string[]> s_AssetBundleDependencies = new Dictionary<string, string[]>();
		public static Dictionary<string, string[]> AssetBundleDependencies {get {return s_AssetBundleDependencies;}}
        static Dictionary<string, LoadedAssetBundle> s_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();

        #region assetbundle downloading
		static string s_BaseDownloadingURL = Config.CdnUrl;

        static Queue<string> s_ToDownloadAssetBundles = new Queue<string>();
        static Dictionary<string, UnityWebRequest> s_DownloadingWWWs = new Dictionary<string, UnityWebRequest>();

        static HandleDownloadCallback m_callback;
        public void SetDownloadCallback(HandleDownloadCallback callback) { m_callback = callback; }

        public static int GetToDownloadAssetBundleNum()
        {
            return s_ToDownloadAssetBundles.Count;
        }
        public static void AddDownloadAssetBundle(string assetBundleName)
        {
            s_ToDownloadAssetBundles.Enqueue(assetBundleName);
        }
        public static int GetDownloadingWWWNum()
        {
            return s_DownloadingWWWs.Count;
        }
        #endregion


        public IEnumerator LoadDependenceInfo()
        {
			Debug.Log ("LoadDependenceInfo start------");
            string filename = Path.Combine(AssetBundleLoader.ASSET_PATH, AssetBundleLoader.GetPlatformFolderForAssetBundles());
            UnityWebRequest www = UnityWebRequest.Get(filename);
            yield return www.SendWebRequest();

            if (www.error != null)
            {
				Debug.LogWarning(www.error + " " + filename);
                www.Dispose();
                yield break;
            }

			//byte[] data = XXTEA.Decrypt(www.bytes);
			AssetBundle assetBundle = AssetBundle.LoadFromMemory(www.downloadHandler.data);
            AssetBundleManifest manifest = assetBundle.LoadAsset("assetbundlemanifest") as AssetBundleManifest;
            string[] assetBundleNames = manifest.GetAllAssetBundles();
            foreach (string assetBundleName in assetBundleNames)
            {
                string[] dependencies = manifest.GetAllDependencies(assetBundleName);
                s_AssetBundleDependencies.Add(assetBundleName, dependencies);
            }
            www.Dispose();
            assetBundle.Unload(true);

			Debug.Log ("LoadDependenceInfo end------");
        }

		public static AssetBundle GetLoadedAssetBundle(string assetBundleName) {

			assetBundleName = assetBundleName.ToLower();

			if (s_LoadedAssetBundles.ContainsKey(assetBundleName))
			{
				return s_LoadedAssetBundles[assetBundleName].assetBundle;
			}
			return null;
		}

        // load assetbuddle 
        // ref++
        public static AssetBundle LoadAssetBundle(string assetBundleName)
        {
            assetBundleName = assetBundleName.ToLower();
            if (s_AssetBundleDependencies.ContainsKey(assetBundleName))
            {
                string[] dependencies = s_AssetBundleDependencies[assetBundleName];
                foreach (string dependency in dependencies)
                {
                    if (s_LoadedAssetBundles.ContainsKey(dependency))
                    {
                        s_LoadedAssetBundles[dependency].refCount++;
                    }
                    else
                    {
                        // 加载
                        string filename = Path.Combine(AssetBundleLoader.ASSET_PATH, dependency);
                        AssetBundle ab = AssetBundle.LoadFromFile(filename);
                        if (ab)
                        {
                            Debug.LogFormat("AssetBundle(Dependency) loaded : {0}", dependency);
                            LoadedAssetBundle loadedAssetBundle = new LoadedAssetBundle(ab);
                            s_LoadedAssetBundles.Add(dependency, loadedAssetBundle);
                            continue;
                        } else {
                            Debug.LogFormat("AssetBundle(Dependency) not found : {0}", filename);
                        }
                    }
                }
            }

            if (s_LoadedAssetBundles.ContainsKey(assetBundleName))
            {

                s_LoadedAssetBundles[assetBundleName].refCount++;

                return s_LoadedAssetBundles[assetBundleName].assetBundle;
            }
            else
            {
                // 加载
                string filename = Path.Combine(AssetBundleLoader.ASSET_PATH, assetBundleName);
                if (File.Exists(filename))
                {
                    AssetBundle ab = null;
                    byte[] bytes = File.ReadAllBytes(filename);
                    if (Utils.IsEncrypted(bytes))
                    {
                        byte[] data = XXTEA.Decrypt(bytes);
                        ab = AssetBundle.LoadFromMemory(data);
                    } else {
                        ab = AssetBundle.LoadFromMemory(bytes);
                    }
                    bytes = null;
                    if (ab)
                    {
                        Debug.LogFormat("AssetBundle loaded : {0}", assetBundleName);
                        LoadedAssetBundle loadedAssetBundle = new LoadedAssetBundle(ab);
                        s_LoadedAssetBundles.Add(assetBundleName, loadedAssetBundle);
                        return s_LoadedAssetBundles[assetBundleName].assetBundle;
                    } else {
                        Debug.LogFormat("AssetBundle not found : {0}", filename);
                    }
                } else {

                    Debug.LogFormat("AssetBundle not found : {0}", filename);
                }

                return null;
            }
        }

        // load assetbuddle Async
        // ref++
        public void LoadAssetBundleAsync(string assetBundleName, LoadAssetBundleAsyncDelegate callback)
        {
            StartCoroutine(_LoadAssetBundleAsync(assetBundleName, callback));
        }
        public IEnumerator _LoadAssetBundleAsync(string assetBundleName, LoadAssetBundleAsyncDelegate callback)
        {
            assetBundleName = assetBundleName.ToLower();
            if (s_AssetBundleDependencies.ContainsKey(assetBundleName))
            {
                string[] dependencies = s_AssetBundleDependencies[assetBundleName];
                foreach (string dependency in dependencies)
                {
                    if (s_LoadedAssetBundles.ContainsKey(dependency))
                    {
                        s_LoadedAssetBundles[dependency].refCount++;
                    }
                    else
                    {
                        // 加载
                        string filename = Path.Combine(AssetBundleLoader.ASSET_PATH, dependency);
                        UnityWebRequest www = UnityWebRequest.Get(filename);
                        Debug.Log("www " + filename);
                        yield return www.SendWebRequest();

                        if (www.error == null)
                        {
							if (s_LoadedAssetBundles.ContainsKey (dependency)) {
								s_LoadedAssetBundles [dependency].refCount++;
								www.Dispose ();
								www = null;

							} else {
								AssetBundle ab = null;
                                if (Utils.IsEncrypted(www.downloadHandler.data))
                                {
                                    byte[] data = XXTEA.Decrypt(www.downloadHandler.data);
                                    ab = AssetBundle.LoadFromMemory(data);
                                    data = null;
                                }
                                else
                                {
                                    ab = AssetBundle.LoadFromMemory(www.downloadHandler.data);
                                }

                                Debug.LogFormat ("AssetBundle(Dependency) loaded : {0}", dependency);
								LoadedAssetBundle loadedAssetBundle = new LoadedAssetBundle (ab);
								s_LoadedAssetBundles.Add (dependency, loadedAssetBundle);
								www.Dispose ();
								www = null;
							}
                        }
                        else
                        {
                            Debug.LogWarning(www.error);
							www.Dispose();
							www = null;
                            yield break;
                        }
                    }
                }
            }

            if (s_LoadedAssetBundles.ContainsKey(assetBundleName))
            {

                s_LoadedAssetBundles[assetBundleName].refCount++;

                callback(s_LoadedAssetBundles[assetBundleName].assetBundle);
            }
            else
            {
                // 加载
                string filename = Path.Combine(AssetBundleLoader.ASSET_PATH, assetBundleName);
                UnityWebRequest www = UnityWebRequest.Get(filename);
				Debug.Log ("www " + filename);
                yield return www.SendWebRequest();

                if (www.error == null)
                {
					if (s_LoadedAssetBundles.ContainsKey(assetBundleName))
					{
						s_LoadedAssetBundles[assetBundleName].refCount++;

						www.Dispose ();
						www = null;
                        callback(s_LoadedAssetBundles[assetBundleName].assetBundle);
					} else {

						AssetBundle ab = null;
						if (Utils.IsEncrypted (www.downloadHandler.data)) {
							byte[] data = XXTEA.Decrypt (www.downloadHandler.data);
							ab = AssetBundle.LoadFromMemory (data);
							data = null;
						} else {
							ab = AssetBundle.LoadFromMemory(www.downloadHandler.data);
                        }

                        //AssetBundle ab = AssetBundle.LoadFromMemory (www.bytes);
                        Debug.LogFormat("AssetBundle loaded : {0}", assetBundleName);
	                    LoadedAssetBundle loadedAssetBundle = new LoadedAssetBundle(ab);
	                    s_LoadedAssetBundles.Add(assetBundleName, loadedAssetBundle);
						www.Dispose();
						www = null;
                        callback(s_LoadedAssetBundles[assetBundleName].assetBundle);
					}
                }
                else
                {
					Debug.LogWarning(www.error + " " + assetBundleName);
					www.Dispose();
					www = null;
                    callback(null);
                }
            }
        }


		public static void UnloadAssetBundle(string assetBundleName, bool unloadAll)
        {
            assetBundleName = assetBundleName.ToLower();
            if (s_AssetBundleDependencies.ContainsKey(assetBundleName))
            {
                string[] dependencies = s_AssetBundleDependencies[assetBundleName];
                foreach (string dependency in dependencies)
                {
                    if (s_LoadedAssetBundles.ContainsKey(dependency))
                    {
                        s_LoadedAssetBundles[dependency].refCount--;
                        if (s_LoadedAssetBundles[dependency].refCount == 0)
                        {
							s_LoadedAssetBundles[dependency].Unload(unloadAll);
                            s_LoadedAssetBundles.Remove(dependency);
                            Debug.LogFormat("AssetBundle unloaded : {0}", dependency);
                        }
                    }
                }
            }

            if (s_LoadedAssetBundles.ContainsKey(assetBundleName))
            {

                s_LoadedAssetBundles[assetBundleName].refCount--;

                if (s_LoadedAssetBundles[assetBundleName].refCount == 0)
                {
					s_LoadedAssetBundles[assetBundleName].Unload(unloadAll);
                    s_LoadedAssetBundles.Remove(assetBundleName);
                    Debug.LogFormat("AssetBundle unloaded : {0}", assetBundleName);
                }
            }
        }

        public static void UnloadAllAssetBundle()
        {
            List<string> assetbundleNames = new List<string>(s_LoadedAssetBundles.Keys);

            for (int i = 0; i < assetbundleNames.Count; i ++)
            {
                UnloadAssetBundle(assetbundleNames[i], true);
            }
            assetbundleNames = new List<string>(s_LoadedAssetBundles.Keys);
            for (int i = 0; i < assetbundleNames.Count; i++)
            {
                Debug.Log(assetbundleNames[i]);
                s_LoadedAssetBundles[assetbundleNames[i]].assetBundle.Unload(true);
            }
            Debug.Log("UnloadAllAssetBundle left: " + s_LoadedAssetBundles.Count);
        }

        public static void Clear() {
            UnloadAllAssetBundle();
            s_LoadedAssetBundles.Clear();
            s_AssetBundleDependencies.Clear();
        }

        private void OnApplicationQuit()
        {
            Clear();
        }

        #region assetbundle download
        private void DownloadAssetBundle(string assetBundleName)
        {
            Debug.Log("DownloadAssetBundle " + assetBundleName);
 
            StartCoroutine(_DownloadAssetBundle(Path.Combine(s_BaseDownloadingURL, assetBundleName), assetBundleName, delegate (UnityWebRequest www)
            {

                // write to local 
                WriteToLocal(assetBundleName, www.downloadHandler.data);
                
            }
            ));
        }

        IEnumerator _DownloadAssetBundle(string url, string assetBundleName, HandleDownloadFinish handler)
        {

            Debug.Log("start downloading " + url);

            UnityWebRequest www = UnityWebRequest.Get(url);
            s_DownloadingWWWs.Add(assetBundleName, www);

            yield return  www.SendWebRequest();

            if (www.error != null)
            {
                Debug.LogErrorFormat("downloading error:{0} - {1}", www.error, url);
            }
            else
            {
                if (www.isDone)
                {
                    if (handler != null)
                    {
                        handler(www);
                    }
                }
            }

            // destroy
            s_DownloadingWWWs.Remove(assetBundleName);
            www.Dispose();

            if (m_callback != null) m_callback(assetBundleName);
        }

        void Update()
        {
            if (s_DownloadingWWWs.Count < 5)
            {
                if (s_ToDownloadAssetBundles.Count > 0)
                {

                    string assetBundleName = s_ToDownloadAssetBundles.Dequeue();
                    DownloadAssetBundle(assetBundleName);
                }
            }
        }

        private void WriteToLocal(string name, byte[] data)
        {
            Debug.Log("WriteToLocal " + name);
            string filename = Path.Combine(AssetBundleLoader.ASSET_PATH_LOCAL, name);
            if (!File.Exists(filename))
            {
                string path = Path.GetDirectoryName(filename);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }

            FileStream file = new FileStream(filename, FileMode.Create);
            file.Write(data, 0, data.Length);
            file.Close();
        }
        #endregion
    }
}