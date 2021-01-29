//using System;
using System.IO;
//using System.Collections;
//using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Framework
{
	[ExecuteInEditMode]
	public class ObjectModify : Editor
	{
		[MenuItem("Tools/Reset Center Position")]
		public static void ResetCenterPosition()
		{
			Transform parent =  Selection.activeGameObject.transform;  
			Vector3 postion = parent.position;  
			Quaternion rotation = parent.rotation;  
			Vector3 scale = parent.localScale;  
			parent.position = Vector3.zero;  
			parent.rotation = Quaternion.identity;  
			parent.localScale = Vector3.one;  
		  
			Renderer[] renders = parent.GetComponentsInChildren<Renderer>();  

			for (int i = 0; i < 5; i++) {
				// 1次不成功？
				Bounds bounds = new Bounds (Vector3.zero, Vector3.zero);  
				foreach (Renderer child in renders) {  
					bounds.Encapsulate (child.bounds);    
				}  
		  
				parent.position = postion;  
				parent.rotation = rotation;  
				parent.localScale = scale;  
		  
				foreach (Transform t in parent) {  
					t.position = t.position - bounds.center;
				}  
			}
			//parent.transform.position = bounds.center + parent.position;
		}
	}
}