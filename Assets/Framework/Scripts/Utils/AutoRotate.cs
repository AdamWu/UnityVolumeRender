using UnityEngine;
using System.Collections;

public class AutoRotate : MonoBehaviour {

	public Vector3 vector;
	public Space space;

	void Start () {
		
	}
	
	void Update () {
		transform.Rotate(vector*Time.deltaTime, space);
	}
}
