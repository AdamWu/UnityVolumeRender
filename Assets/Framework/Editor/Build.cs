using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Framework
{
    [ExecuteInEditMode]
	class Build : Editor
	{

		static string[] _scenes = {
			// public scene 
		};

		static string pathToBuild = Application.dataPath + "/../Exports";

		[MenuItem("Build/Build Scenes x86")]
		static void BuildScenesx86() {	
			Stack<string> dirs = new Stack<string>();
			dirs.Push("Assets/Scenes");
			while (dirs.Count > 0)
			{
				string dir = dirs.Pop();
				// files
				foreach (string file in Directory.GetFiles(dir))
				{
					if (!file.EndsWith(".unity"))
						continue;

					Debug.Log (file);
					BuildScene (file);
				}
				// child dirs
				foreach (string path in Directory.GetDirectories(dir))
				{
					dirs.Push(path);
				}
			}
		}

		static void BuildScene(string path) {
			Debug.Log("BuildScene " + path);

			string[] scenes = new string[_scenes.Length+1];
			for (int i = 0; i < _scenes.Length; i++) {
				scenes [i] = _scenes [i];
			}
			scenes [scenes.Length - 1] = path;

			string name = PlayerSettings.productName;
			string identifier = PlayerSettings.applicationIdentifier;

			string targetName = Path.GetFileNameWithoutExtension(path);
			string targetPath = Path.Combine(pathToBuild, "PC");

			if (!Directory.Exists (targetPath)) {
				Directory.CreateDirectory(targetPath);
			}
				
			PlayerSettings.productName = targetName;

			// switch target
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows);
			string exe = Path.Combine (targetPath, targetName+".exe");
			BuildPipeline.BuildPlayer(scenes , Path.GetFullPath (exe), BuildTarget.StandaloneWindows, BuildOptions.None);
		}

		/// <summary>
		/// Deletes the folder.
		/// </summary>
		/// <param name="dir">Dir.</param>
		public static void DeleteFolder(string dir) {
			foreach (string d in Directory.GetFileSystemEntries(dir)) {
				if (File.Exists(d)) {
					FileInfo fi = new FileInfo(d);
					if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
						fi.Attributes = FileAttributes.Normal;
					File.Delete(d);
				} else {
					DirectoryInfo d1 = new DirectoryInfo(d);
					if (d1.GetFiles().Length != 0) {
						DeleteFolder(d1.FullName);
					}
					Directory.Delete(d);
				}
			}
		}

		/// <summary>
		/// Copies the directory.
		/// </summary>
		/// <param name="srcPath">Source path.</param>
		/// <param name="destPath">Destination path.</param>
		public static void CopyDirectory(string srcPath, string dstPath) {
			if (!Directory.Exists(dstPath)) {
				Directory.CreateDirectory(dstPath);
			}

			DirectoryInfo info = new DirectoryInfo(srcPath);
			// files
			foreach (FileInfo file in info.GetFiles()){
				file.CopyTo(Path.Combine(dstPath, file.Name), true);
			}
			// dirs
			foreach (DirectoryInfo dir in info.GetDirectories()){
				CopyDirectory(dir.FullName, Path.Combine(dstPath, dir.Name));
			}
		}
	}
}