using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Framework
{
    public delegate void LoadAssetAsyncDelegate(Object obj);

    public class ResourceManager : Singleton<ResourceManager>
    {

        public void Init()
        {
            Debug.Log("ResourceManager:Init");
        }

		public Object GetLoadedAsset(string assetBundleName) {
            /*
			if (!Config.UseAssetbundle) {
				int pos = assetBundleName.LastIndexOf ('.');
				assetBundleName = assetBundleName.Substring (0, pos);
				Object obj = Resources.Load (assetBundleName);
				return obj;
			}
            */

			if (!assetBundleName.EndsWith(".assetbundle")) {
				assetBundleName += ".assetbundle";
			}
			AssetBundle assetBundle = AssetBundleManager.GetLoadedAssetBundle(assetBundleName);
			if (assetBundle != null) {
				return assetBundle.LoadAsset (assetBundle.GetAllAssetNames () [0]);
			} else {
				return null;
			}
		}

        /*
		 * 暂不支持同步加载（webgl不支持）
         */
        public Object LoadAsset(string assetBundleName, string assetName="")
        {
#if !UNITY_WEBGL
            if (!assetBundleName.EndsWith(".assetbundle"))
            {
                assetBundleName += ".assetbundle";
            }

            AssetBundle assetBundle = AssetBundleManager.LoadAssetBundle(assetBundleName);
            if (assetBundle != null)
            {
                if (assetName.Equals(""))
                {
                    Object asset = assetBundle.LoadAsset(assetBundle.GetAllAssetNames()[0]);
                    return asset;
                } else
                {
                    Object asset = assetBundle.LoadAsset(assetName);
                    return asset;
                }
            }
            return null;
#else
            Debug.LogWarning("Not Supported in WebGL.");
            return null;
#endif
        }
        
        public void LoadAssetAsync(string assetBundleName, LoadAssetAsyncDelegate callback)
        {
			if (!Config.UseAssetbundle) {
				Object obj = Resources.Load (assetBundleName);
				callback (obj);
				return;
			}
			
			if (!assetBundleName.EndsWith(".assetbundle")) {
				assetBundleName += ".assetbundle";
			}
            AssetBundleManager.Instance.LoadAssetBundleAsync(assetBundleName, delegate (AssetBundle assetBundle)
            {
                if (assetBundle != null)
                {
                    Object asset = assetBundle.LoadAsset(assetBundle.GetAllAssetNames()[0]);
                    callback(asset);
                } else {
                    callback(null);
                }
            });
        }

        public IEnumerator _LoadAssetAsync(string assetBundleName, LoadAssetAsyncDelegate callback)
        {
            if (!Config.UseAssetbundle)
            {
                Object obj = Resources.Load(assetBundleName);
                callback(obj);
                yield return null;
            }
            else
            {
                // assetbundle
                if (!assetBundleName.EndsWith(".assetbundle"))
                {
                    assetBundleName += ".assetbundle";
                }
                yield return StartCoroutine(AssetBundleManager.Instance._LoadAssetBundleAsync(assetBundleName, delegate (AssetBundle assetBundle)
                {
                    if (assetBundle != null)
                    {
                        Object asset = assetBundle.LoadAsset(assetBundle.GetAllAssetNames()[0]);
                        callback(asset);
                    }
                    else
                    {
                        callback(null);
                    }
                }));
            }
        }

        public void UnloadAsset(string assetBundleName, bool unloadAll)
		{
			if (!assetBundleName.EndsWith(".assetbundle")) {
				assetBundleName += ".assetbundle";
			}
			AssetBundleManager.UnloadAssetBundle(assetBundleName, unloadAll);
        }

        public static long GetFileSize(string filename)
        {
            if (!File.Exists(filename))
            {
                Debug.LogFormat("GetFileSize: {0} not Exist!", filename);
                return 0;
            }

            FileStream fs = new FileStream(filename, FileMode.Open);
            long length = fs.Length;
            fs.Close();
            return length;
        }
        public static string GetFileHash(string filename)
        {
            if (!File.Exists(filename))
            {
                Debug.LogFormat("GetFileHash: {0} not Exist!", filename);
                return null;
            }

            FileStream fs = new FileStream(filename, FileMode.Open);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, (int)fs.Length);
            Hash128 hash = Hash128.Parse(data.ToString());
            fs.Close();
            return hash.ToString();
        }
    }
}