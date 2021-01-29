//using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Text;

namespace Framework
{
    [ExecuteInEditMode]
    public class AssetBundleBuild : Editor
    {
        const string kAssetBundlesOutputPath = "Assets/StreamingAssets/";

        public static string GetPlatformFolderForAssetBundles(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.WebGL:
                    return "WebGL";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                default:
                    return null;
            }
        }

        [MenuItem("AssetBundle/Build All")]
        private static void BuildAll()
		{
			Debug.Log("Build AssetBundles ...");

            string PlatformFolder = GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
            string path = Path.Combine(kAssetBundlesOutputPath, PlatformFolder);

			// 新建文件夹
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(path, 0, EditorUserBuildSettings.activeBuildTarget);
            if (manifest == null)
            {
                EditorUtility.DisplayDialog("警告", "没有需要打包的assetbundle，请先设置assetbundle的名字！", "确定");
                return;
            }

            // generate resourcelist
            string filename = Path.Combine(path, "version.txt");
            FileStream fs = new FileStream(filename, FileMode.Create);
            StreamWriter writer = new StreamWriter(fs);
            writer.WriteLine(string.Format("version 1.0.0"));
            writer.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // write assetbundles
            string[] assets = manifest.GetAllAssetBundles();
            foreach (string asset in assets)
            {
                // assetbundle 
                Hash128 hash = manifest.GetAssetBundleHash(asset);
                long size = ResourceManager.GetFileSize(Path.Combine(path, asset));
                writer.WriteLine(string.Format("{0} {1} {2}", asset, hash.ToString(), size));
            }

            writer.Close();
            fs.Close();

            EncryptFilesRecursively(path);

			AssetDatabase.Refresh ();

            Debug.Log("Build AssetBundles ok!");
        }

		[MenuItem("AssetBundle/Clear StreamimgAssets")]
		private static void ClearSteamingAssets()
		{
			Debug.Log("Build AssetBundles ...");

			string PlatformFolder = GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
			string path = Path.Combine(kAssetBundlesOutputPath, PlatformFolder);

			// 清空文件夹
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}

			AssetDatabase.Refresh ();

			Debug.Log("ClearSteamingAssets ok!");
		}

        
        private static void EncryptFilesRecursively(string path)
        {
            // files
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
				string ext = Path.GetExtension (file);
				if (ext == ".assetbundle" || ext == ".unity3d")
				{
					EncryptFile(file);
				}
            }

            // dirs recusively
            string[] dirs = Directory.GetDirectories(path);
            for (int i = 0; i < dirs.Length; i++)
            {
                EncryptFilesRecursively(dirs[i]);
            }
        }

        private static void EncryptFile(string file)
        {
            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.ReadWrite);

            int numBytesToRead = (int)fs.Length;
            int numBytesRead = 0;
            byte[] readByte = new byte[fs.Length];
            //读取字节
            while (numBytesToRead > 0)
            {
                // Read may return anything from 0 to numBytesToRead.
                int n = fs.Read(readByte, numBytesRead, numBytesToRead);

                // Break when the end of the file is reached.
                if (n == 0)
                    break;

                numBytesRead += n;
                numBytesToRead -= n;
            }
            fs.Close();

			// 判断是否已加密
			if (Utils.IsEncrypted (readByte)) {
				return;
			}

            //加密
            byte[] newBuff = XXTEA.Encrypt(readByte);

            // 保存
            FileStream cfs = new FileStream(file, FileMode.Create);
            cfs.Write(newBuff, 0, newBuff.Length);
            newBuff = null;
            cfs.Close();

			Debug.Log("encrypt file:" + file);

        }

        private static void DeleteEmptyFolder(string path)
        {
            foreach (string dir in Directory.GetDirectories(path))
            {
                DeleteEmptyFolder(dir);
            }

            string[] paths = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);
            if (files.Length == 0 && paths.Length == 0)
            {
                Directory.Delete(path);
            }
        }

		[MenuItem("AssetBundle/Gen Bundle Names In Resources")]
		private static void GenBundleNamesInResources()
		{
			Stack<string> dirs = new Stack<string>();
			dirs.Push("Assets/_Resources/");
			while (dirs.Count > 0)
			{
				string dir = dirs.Pop();
				if (!Directory.Exists (dir))
					continue;
				
				// files
				foreach (string file in Directory.GetFiles(dir))
				{
					string bundleName = file.Substring(file.IndexOf ("/")+1);
					bundleName = bundleName.Substring(bundleName.IndexOf ("/")+1);
					bundleName = bundleName.Substring(0, bundleName.IndexOf("."));
					bundleName += ".assetbundle";

					AssetImporter importer = AssetImporter.GetAtPath(file);
					if (importer)
					{
						importer.assetBundleName = bundleName;
						importer.assetBundleVariant = null;
					}
				}
				// child dirs
				foreach (string path in Directory.GetDirectories(dir))
				{
					dirs.Push(path);
				}
			}
			AssetDatabase.RemoveUnusedAssetBundleNames();
			Debug.Log("GenBundleNamesInResources success!");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

        [MenuItem("AssetBundle/Gen Bundle Names In Scenes")]
        private static void GenSceneBundleNames()
        {
            Stack<string> dirs = new Stack<string>();
            dirs.Push("Assets/Scenes/");
            while (dirs.Count > 0)
            {
                string dir = dirs.Pop();
                // files
                foreach (string file in Directory.GetFiles(dir))
                {
                    if (!file.EndsWith(".unity"))
                        continue;
					
					string bundleName = file.Substring(file.IndexOf ("/")+1);
					bundleName = bundleName.Substring(bundleName.IndexOf ("/")+1);
					bundleName = bundleName.Substring(0, bundleName.IndexOf("."));
					bundleName += ".unity3d";
                    AssetImporter importer = AssetImporter.GetAtPath(file);
                    if (importer)
					{
						importer.assetBundleName = bundleName;
						importer.assetBundleVariant = null;
                    }
                }
                // child dirs
                foreach (string path in Directory.GetDirectories(dir))
                {
                    dirs.Push(path);
                }
            }

			Debug.Log("GenSceneBundleNames success!");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

		[MenuItem("AssetBundle/Clear All Bundle Names")]
		private static void ClearAllBundleNames()
		{
			string[] bundleNames = AssetDatabase.GetAllAssetBundleNames ();
			for (int i = 0; i < bundleNames.Length; i++) {
				AssetDatabase.RemoveAssetBundleName (bundleNames [i], true);
			}
			AssetDatabase.RemoveUnusedAssetBundleNames();
			Debug.Log("ClearAllBundleNames success!");

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}


        [MenuItem("Assets/AddSelectSceneNames")]
        private static void AddSelectSceneNames()
        {
            Stack<string> dirs = new Stack<string>();
            Object selectedObject = Selection.activeObject;
            string temppath = AssetDatabase.GetAssetPath(selectedObject);
            dirs.Push(temppath);
            while (dirs.Count > 0)
            {
                string dir = dirs.Pop();
                // files
                foreach (string file in Directory.GetFiles(dir))
                {
					if (!file.EndsWith (".unity")) 
					{
						
					} 
					else 
					{
						AssetImporter importer = AssetImporter.GetAtPath (file);
						if (importer) {
							string[] temp = file.Split (new char[] { '/', '\\' });
							Debug.Log (temp [temp.Length - 1]);
							importer.assetBundleName = temp [temp.Length - 1].Replace (".unity", ".unity3d");
						}
					}
                }
                if (!File.Exists(dir))//判断选中的是否是文件
                {
                    foreach (string path in Directory.GetDirectories(dir))
                    {
                        dirs.Push(path);
                    }
                }
            }

			Debug.Log("AddSelectSceneNames success");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}