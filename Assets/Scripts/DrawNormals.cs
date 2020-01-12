using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawNormals : MonoBehaviour {


	// Use this for initialization
	void Awake () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnDrawGizmos()
    {
        Mesh mf = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mf.vertices;
        if (vertices == null)
        {
            return;
        }
        Gizmos.color = Color.black;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawLine(mf.normals[i], Vector3.up);
        }
    }
}
