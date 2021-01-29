
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Framework
{
	public class Utils {

		// Find GameObject by name
		// 解决gameobject隐藏，GameObject.Find无法找到
		public static GameObject FindGameObject(string name) {
			GameObject go = GameObject.Find (name);
			if (go == null) {
				string[] strs = name.Split ('/');
				if (strs.Length > 1) {
					go = GameObject.Find (strs[0]);
					Transform tf = go.transform.Find (strs [1]);
					go = tf.gameObject;
				}
			}
			return go;
		}


        public static void SetLayerRecusively(GameObject gameObject, int layer)
        {
            if (gameObject == null) return;

            gameObject.layer = layer;

            foreach(Transform child in gameObject.transform) {
                SetLayerRecusively(child.gameObject, layer);
            }
           
        }

		public static bool IsEncrypted(byte[] data) {
			byte[] header = new byte[5];
			for (int i = 0; i < 5; i++) {
				header [i] = data [i];
			}
			if (System.Text.Encoding.Default.GetString (header) != "Unity") {
				return true;
			}
			return false;
		}
	}
}