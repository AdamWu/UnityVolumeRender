/*
 * Mesh Combine Dynamic
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
public class CombineMeshes : MonoBehaviour
{	
	/// combine material or not
	/// it will combine all materail to one materail(one atlas and one shader)!!!
	/// use carefully!
	public bool m_CombineMaterial = false;

	void Start()
	{
		MeshRenderer[] meshRenders = GetComponentsInChildren<MeshRenderer>();

		// 统计材质、贴图数
		Dictionary<Material, List<int>> dic_material_render = new Dictionary<Material, List<int>>();
		for (int i = 0; i < meshRenders.Length; i++) {

			Material mat = meshRenders[i].sharedMaterial;
			if (dic_material_render.ContainsKey(mat)) {
				dic_material_render[mat].Add(i);
				continue;
			}

			dic_material_render.Add(mat, new List<int>());
			dic_material_render[mat].Add(i);
		}

		if (m_CombineMaterial) {
			CombineAllMaterial (dic_material_render);
		} else {
			CombineSameMaterial (dic_material_render);
		}
	}

	// 合并相同材质mesh
	void CombineSameMaterial(IDictionary dicMat2Render) {

		Matrix4x4 myTransform = transform.worldToLocalMatrix;

		Dictionary<Material, List<int>> dic_material_render = dicMat2Render as Dictionary<Material, List<int>>;

		//合并相同材质Mesh
		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
		foreach (Material mat in dic_material_render.Keys) {
			List<int> renders = dic_material_render[mat];
			CombineInstance[] combine = new CombineInstance[renders.Count];
			for (int j = 0; j < renders.Count; j ++) {
				int idx = renders[j];
				MeshFilter meshFilter = meshFilters[idx];
				combine[j].mesh = meshFilter.sharedMesh;
				combine[j].transform = myTransform * meshFilter.transform.localToWorldMatrix;
				meshFilter.gameObject.SetActive(false);
			}

			GameObject go = new GameObject("Combined Mesh");
			go.transform.SetParent(transform);
			go.transform.localScale = Vector3.one;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localPosition = Vector3.zero;
			MeshFilter filter = go.AddComponent<MeshFilter>();
			filter.mesh = new Mesh();
			filter.mesh.CombineMeshes(combine, true, true);
			MeshRenderer render = go.AddComponent<MeshRenderer>();
			render.sharedMaterial = mat;
		}
	}

	// 合并所有mesh，且合并材质
	void CombineAllMaterial(IDictionary dicMat2Render) {
		
		Matrix4x4 myTransform = transform.worldToLocalMatrix;
		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

		Dictionary<Material, List<int>> dic_material_render = dicMat2Render as Dictionary<Material, List<int>>;

		Material materialOld = new List<Material>(dic_material_render.Keys)[0];

		// 合并材质、贴图
		Texture2D[] textures = new Texture2D[dic_material_render.Count];
		int i = 0;
		foreach (Material mat in dic_material_render.Keys) {
			Texture2D tx = mat.GetTexture("_MainTex") as Texture2D;
			textures[i] = tx;
			i ++;
		}

		// add New Mesh
		MeshRenderer meshRenderSelf = gameObject.AddComponent<MeshRenderer>();
		MeshFilter meshFilterSelf = gameObject.AddComponent<MeshFilter>();
	
		Material materialNew = new Material(materialOld.shader);
		materialNew.CopyPropertiesFromMaterial(materialOld);
		meshRenderSelf.sharedMaterial = materialNew;

		Texture2D texture = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);
		materialNew.SetTexture("_MainTex", texture);
		Rect[] rects = texture.PackTextures(textures, 0);

		// 重置uv
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		i = 0;
		foreach (Material mat in dic_material_render.Keys) {
			Rect rect = rects[i];
			i ++;

			List<int> renders = dic_material_render[mat];
			for (int j = 0; j < renders.Count; j ++) {
				int idx = renders[j];
				MeshFilter meshFilter = meshFilters[idx];
				Mesh meshCombine = meshFilter.mesh;
				Vector2[] uvs = new Vector2[meshCombine.uv.Length];

				for (int k = 0; k < uvs.Length; k++) {
					uvs[k].x = rect.x + meshCombine.uv[k].x * rect.width;
					uvs[k].y = rect.y + meshCombine.uv[k].y * rect.height;
				}
				meshCombine.uv = uvs;
				combine[idx].mesh = meshCombine;
				combine[idx].transform = myTransform * meshFilter.transform.localToWorldMatrix;
				meshFilters[idx].gameObject.SetActive(false);
			}
		}

		Mesh newMesh = new Mesh();
		newMesh.CombineMeshes(combine, true, true);//全部合并
		meshFilterSelf.mesh = newMesh;
	}
 
}