using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Mi serve una MeshFilter per avere la composizione della mesh, e un MeshRenderer per ereditare i materiali dell'oggetto padre
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshShatterer : MonoBehaviour
{
    private HalfEdgeHelper.HalfEdge[] HE;
    private Dictionary<int, List<int>> fragmentsFaces;

    public void ExplosionHandler(int fragments, HalfEdgeHelper.HalfEdge[] helper, Dictionary<int, List<int>> faces)
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        // se la MeshFilter è null, non far nulla
        if (mf == null)
        {
            //yield return null;
        }
        else
        {
            // recupero la mesh del GameObject originale
            Mesh originalMesh = GetComponent<MeshFilter>().mesh;
            HE = helper;
            fragmentsFaces = faces;

            if (fragments >= originalMesh.triangles.Length / 3)
            {
                Explode_max(mf);
            }
            else
            {
                Explode(mf, fragments);
            }
        }
    }



    //public IEnumerator Explode()
    public void Explode_max(MeshFilter mf)
    {
        // Disattivo il collider se ne ha uno
        if (GetComponent<Collider>())
        {
            GetComponent<Collider>().enabled = false;
        }

        // recupero la mesh del GameObject originale
        Mesh originalMesh = mf.mesh;

        // recupero i materiali del GameObject originale
        Material[] materials = GetComponent<MeshRenderer>().materials;

        // recupero vertici e normali della mesh originale
        Vector3[] verts = originalMesh.vertices;
        Vector3[] normals = originalMesh.normals;
        Vector2[] uvs = new Vector2[] { };
        // se aveva una texture, recupero anche quella
        if (originalMesh.uv != null)
        {
            uvs = originalMesh.uv;
        }

        // La mesh potrebbe avere delle submesh, ognuna con il proprio materiale. Le elaboro tutte?
        for (int submesh = 0; submesh < originalMesh.subMeshCount; submesh++)
        {

            int[] triangles = originalMesh.GetTriangles(submesh);
            //Debug.Log("Triangoli: " + triangles.Length);

            // per ogni triangolo creo un nuovo gameobject.
            Vector3[] newVerts = new Vector3[3];
            Vector3[] newNormals = new Vector3[3];
            Vector2[] newUvs = new Vector2[3];
            // scorro i vertici a 3 a 3.
            for (int i = 0; i < triangles.Length; i += 3)
            {
                for (int n = 0; n < 3; n++)
                {
                    int index = triangles[i + n];
                    // i vertici sono gli stessi del triangolo originale
                    newVerts[n] = verts[index];
                    // se c'è una texture, la rimappo sul frammento
                    if (uvs.Length != 0)
                    {
                        newUvs[n] = uvs[index];
                    }

                    // eredito anche le normali
                    newNormals[n] = normals[index];
                }

                Mesh mesh = new Mesh();
                mesh.vertices = newVerts;
                mesh.normals = newNormals;
                mesh.uv = newUvs;

                // creo due nuovi triangoli con un davanti e un dietro per renderizzare anche l'interno del frammento
                mesh.triangles = new int[] { 0, 1, 2, 2, 1, 0 };

                GameObject GO = new GameObject("Fragment " + ((i + 1)));
                // Eredito posizione e rotazione del game object originale
                GO.transform.position = transform.position;
                GO.transform.rotation = transform.rotation;
                GO.transform.localScale = transform.localScale;

                // se uso la transform.parent per dare al mio oggetto la gerarchia giusta, mi eredita anche la scala dell'oggetto padre... perché?
                //  GO.transform.parent = this.transform;


                // ricalcolo le normali -> sembra dare risultati errati rispetto all'ereditare le normali dell'oggetto padre
                //mesh.RecalculateNormals();

                // Eredito anche i materiali (se ci sono più submesh, eredito quelli dell'eventuale submesh originale)
                GO.AddComponent<MeshRenderer>().material = materials[submesh];
                GO.AddComponent<MeshFilter>().mesh = mesh;
                // Aggiungo un collider per poter poi suddividere ulteriormente la mesh.
                //GO.AddComponent<BoxCollider>();
                GO.AddComponent<Rigidbody>();
            }
        }
        //}

        // disattivo la renderizzazione dell'oggetto padre
        GetComponent<Renderer>().enabled = false;

        // dopo un secondo distruggo l'oggetto padre
        //yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }

    public void Explode(MeshFilter mf, int fragments)
    {

        // eredito il collider se ne ha uno
        if (GetComponent<Collider>())
        {
            Collider coll = GetComponent<Collider>();
            coll.enabled = false;
        }

        // recupero la mesh del GameObject originale
        Mesh originalMesh = mf.mesh;

        // recupero i materiali del GameObject originale
        Material material = GetComponent<MeshRenderer>().material;

        // recupero vertici e normali della mesh originale
        Vector3[] verts = originalMesh.vertices;
        Vector3[] normals = originalMesh.normals;
        Vector2[] uvs = new Vector2[] { };
        // se aveva una texture, recupero anche quella
        if (originalMesh.uv != null)
        {
            uvs = originalMesh.uv;
        }

        // arrivato qui ho chiuso anche tutti i buchi. Ora creo i nuovi gameobject
        for (int j = 0; j < fragments; j++)
        {
            // creo la mia nuova mesh 
            Mesh mesh = new Mesh();
            List<Vector3> newVerts = new List<Vector3>();
            List<Vector3> newNormals = new List<Vector3>();
            List<int> newTriangles = new List<int>();
            List<Vector2> newUvs = new List<Vector2>();

            int loops = fragmentsFaces[j].Count;

            // per ogni frammento devo recuperare i vertici a partire dai vertici originali, ma devo fare un remapping dell'indice
            for (int y = 0; y < loops; y ++)
            {
                int t1 = fragmentsFaces[j][y];
                int idx = 0;
                for (int x = 0; x < 3; x++)
                {
                    int v = HE[t1 + x].initialVertex;
                    if (!newVerts.Contains(verts[v]))
                    {
                        newVerts.Add(verts[v]);
                        // se c'è una texture, la rimappo sul frammento
                        if (uvs.Length != 0)
                        {
                            newUvs.Add(uvs[v]); 
                        }
                        newNormals.Add(normals[v]);
                    }
                    idx = newVerts.IndexOf(verts[v]);
                    newTriangles.Add(idx);

                }
            }
            
            mesh.vertices = newVerts.ToArray();

            // creo anche la faccia posteriore del triangolo per renderizzare anche l'interno del frammento
            //for (int t = newTriangles.Count; t > 0; t--)
            //{
            //    newTriangles.Add(newTriangles[t - 1]);
            //    newNormals.Add(normals[t - 1] * -1);
            //}

            mesh.triangles = newTriangles.ToArray();
            mesh.uv = newUvs.ToArray();
            mesh.normals = newNormals.ToArray();
            // ricalcolo le normali -> sembra dare risultati errati rispetto all'ereditare le normali dell'oggetto padre
            //mesh.RecalculateNormals();
            //mesh.RecalculateTangents();

            GameObject GO = new GameObject("Fragment " + ((j + 1)));

            // sarebbe più opportuno creare il frammento come figlio del GO originale?
            // Eredito posizione, rotazione e scala del game object originale
            GO.transform.position = transform.position;
            GO.transform.rotation = transform.rotation;
            GO.transform.localScale = transform.localScale;

            // se uso la transform.parent per dare al mio oggetto la gerarchia giusta, mi eredita anche la scala dell'oggetto padre... perché?
            //GO.transform.parent = this.transform;

            // Eredito anche i materiali 
            GO.AddComponent<MeshRenderer>().material = material;
            GO.AddComponent<MeshFilter>().mesh = mesh;

            GO.AddComponent<BoxCollider>();
            //GO.AddComponent<MeshCollider>();
            //GO.GetComponent<BoxCollider>().size = new Vector3(.015f, .015f, .015f);
            GO.AddComponent<Rigidbody>();

        }

        // disattivo la renderizzazione dell'oggetto padre
        GetComponent<Renderer>().enabled = false;

        // distruggo l'oggetto padre
        Destroy(gameObject);
    }




}
