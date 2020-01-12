using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OLD_MeshShatterer))]
public class OLD_ExplodingGameObject : MonoBehaviour
{
    [Range(1, 1500)]
    public int fragments;
    private HalfEdgeHelper.HalfEdge[] HE;

    // Use this for initialization
    void Start()
    {
        Mesh originalMesh = GetComponent<MeshFilter>().mesh;
        HE = HalfEdgeHelper.GetHalfEdges(originalMesh.vertices, originalMesh.triangles);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(inputRay, out hit))
        {
            return;

        }
        else
        {
            //StartCoroutine(gameObject.GetComponent<MeshShatterer>().Explode_new(fragments, hit.triangleIndex * 3));
            //StartCoroutine(gameObject.GetComponent<MeshShatterer>().ExplosionHandler(fragments);
            //gameObject.GetComponent<MeshShatterer>().ExplosionHandler(fragments);
            gameObject.GetComponent<OLD_MeshShatterer>().ExplosionHandler(fragments, HE);
            Debug.Log("Spacco!");
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint p in collision.contacts)
        {
            RaycastHit hit;
            Ray ray = new Ray(p.point + p.normal * 0.05f, -p.normal);
            if (p.otherCollider.Raycast(ray, out hit, 1.5f))
            {
                //gameObject.GetComponent<MeshShatterer>().ExplosionHandler(fragments);
                gameObject.GetComponent<OLD_MeshShatterer>().ExplosionHandler(fragments, HE);
                break;
            }
        }
    }
}

