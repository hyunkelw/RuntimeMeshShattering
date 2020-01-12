using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(MeshShatterer), typeof(MeshFilter))]
public class ExplodingGameObject : MonoBehaviour
{
    [Range(1, 1500)]
    public int fragments;
    public bool writeDebugFiles;
    public string myDebugfileHE;
    public string myDebugfileVerts;
    public string myDebugfileTriangles;
    private HalfEdgeHelper.HalfEdge[] HE;
    private Vector3[] verts;
    private int[] triangles;
    private HashSet<int> visitedTriangles;
    private Queue<int> trianglesQueue;
    private Dictionary<int, List<int>> fragmentsFaces;

    public bool writeDebugPerformance;
    public string myDebugfilePerf;
    private long elapsedHE;
    private long elapsedBFS;
    private long elapsedEX;

    void Awake()
    {
        Mesh originalMesh = GetComponent<MeshFilter>().mesh;
        verts = originalMesh.vertices;
        triangles = originalMesh.triangles;

        var watch = System.Diagnostics.Stopwatch.StartNew();
        HE = HalfEdgeHelper.GetHalfEdges(verts, triangles);
        watch.Stop();
        elapsedHE = watch.ElapsedMilliseconds;

        watch = System.Diagnostics.Stopwatch.StartNew();
        visitedTriangles = new HashSet<int>();
        trianglesQueue = new Queue<int>();

        // per ora forzo il triangolo 0 come punto di partenza
        //int collidingTriangle = 0;
        int collidingTriangle = UnityEngine.Random.Range(0, triangles.Length);

        // per ogni FRAMMENTO creo un nuovo gameobject.
        // Versione 1. divido in parti uguali la mia mesh per il numero di frammenti che voglio creare
        int trianglesXFragment = Mathf.FloorToInt((triangles.Length / 3) / fragments);

        // parto dal mio triangolo x. recupero il primo dei 3 edges collegati
        int pos = Array.FindIndex(HE, halfEdge => halfEdge.faceIndex == collidingTriangle);

        if (pos >= 0)
        {
            trianglesQueue.Enqueue(pos);
        }

        List<int> availableTriangles = new List<int>();
        // riempio la mia lista di triangoli disponibili con tutti i triangoli possibili
        for (int i = 0; i < triangles.Length; i += 3)
        {
            availableTriangles.Add(i);
        }

        fragmentsFaces = new Dictionary<int, List<int>>();

        List<int> newFaces;

        // per ogni frammento
        for (int j = 0; j < fragments; j++)
        {
            // azzero il mio contatore triangoli
            int trianglesCount = 0;

            newFaces = new List<int>();

            // se il frammento precedente è finito in un vicolo cieco, prendo il primo triangolo disponibile
            if (trianglesQueue.Count == 0)
            {
                trianglesQueue.Enqueue(availableTriangles[0]);
            } // altrimenti prenderà un qualunque triangolo che stavo già per analizzare

            // finché ho elementi nella coda dei triangoli da visitare
            while (trianglesQueue.Count > 0)
            {
                // esamino il prossimo triangolo
                int t1 = trianglesQueue.Dequeue();

                // incremento di 1 il mio contatore
                trianglesCount++;

                // segno il mio triangolo fra i triangoli visitati, e allo stesso tempo lo tolgo dai triangoli ancora disponibili
                visitedTriangles.Add(t1);
                availableTriangles.Remove(t1);
                newFaces.Add(t1);

                // se ho raggiunto il numero di triangoli previsto dal mio frammento, esco
                if (trianglesCount == trianglesXFragment)
                {
                    if (trianglesQueue.Count > 0)
                    {
                        t1 = trianglesQueue.Dequeue();
                        trianglesQueue.Clear();
                        trianglesQueue.Enqueue(t1);
                    }
                    break;
                }

                // dagli edge esamino i triangoli accanto. Parto sempre da un triangolo diverso per aggiungere casualità alla forma del frammento
                //int offset = UnityEngine.Random.Range(0, 3);
                for (int x = 0; x < 3; x++)
                {
                    int crossingEdge = HE[t1 + x].twinEdge;

                    // recupero l'indice della faccia del triangolo interno all'edge appena recuperato
                    int t2 = HE[crossingEdge].faceIndex;
                    // controllo se il triangolo in questione sia già stato visitato (o sia già in coda)
                    if (!visitedTriangles.Contains(t2) && !trianglesQueue.Contains(t2))
                    {
                        // se non è stato visitato e non è in coda, lo aggiungo al mio elenco
                        trianglesQueue.Enqueue(t2);
                    }
                }
            }

            // arrivato qui, ho creato un certo frammento X. Lo registro nel mio array dei frammenti
            fragmentsFaces.Add(j, newFaces);
        }

        // ripulisco la coda
        trianglesQueue.Clear();

        // arrivato a questo punto, potrebbero essere rimasti dei buchi. devo recuperarli e aggiungerli a uno dei frammenti sopracitati
        while (availableTriangles.Count > 0)
        {
            bool found = false;
            // prendo il primo triangolo disponibile
            trianglesQueue.Enqueue(availableTriangles[0]);

            newFaces = new List<int>();

            // finché ho elementi nella coda dei triangoli da visitare
            while (trianglesQueue.Count > 0)
            {

                // esamino il prossimo triangolo
                int t1 = trianglesQueue.Dequeue();

                // segno il mio triangolo fra i triangoli visitati, e allo stesso tempo lo tolgo dai triangoli ancora disponibili
                visitedTriangles.Add(t1);
                availableTriangles.Remove(t1);
                newFaces.Add(t1);

                // dagli edge esamino i triangoli accanto.
                //int offset = UnityEngine.Random.Range(0, 3);
                for (int x = 0; x < 3; x++)
                {
                    int crossingEdge = HE[t1 + x].twinEdge;

                    // recupero l'indice di partenza del triangolo interno all'edge appena recuperato
                    int t2 = HE[crossingEdge].faceIndex;
                    // controllo se il triangolo in questione sia già stato visitato (o sia già in coda)
                    if (!visitedTriangles.Contains(t2))
                    {
                        // se non è stato visitato, lo aggiungo al mio elenco
                        trianglesQueue.Enqueue(t2);
                    }
                    else // se invece è stato visitato, sarà associabile ad un determinato frammento
                    {
                        for (int y = 0; y < fragmentsFaces.Count; y++)
                        {
                            if (fragmentsFaces[y].Contains(t2))
                            {
                                fragmentsFaces[y].AddRange(newFaces);
                                found = true;
                                trianglesQueue.Clear();
                                break;
                            }
                        }
                        break;
                    }
                }
                if (found)
                {
                    break;
                }
            }
        }
        watch.Stop();
        elapsedBFS = watch.ElapsedMilliseconds;

        if (writeDebugFiles)
        {
            writeDebug();
        }

    }

    private void writeDebug()
    {
        if (myDebugfileVerts == null)
        {
            myDebugfileVerts = "/Debug Unity/RunTimeMeshShattering_Verts.csv";
        }
        if (myDebugfileTriangles == null)
        {
            myDebugfileTriangles = "/Debug Unity/RunTimeMeshShattering_Triangles.csv";
        }

        if (myDebugfileHE == null)
        {
            myDebugfileHE = "/Debug Unity/RunTimeMeshShattering_HE.csv";
        }

        try
        {
            Writer wrVerts = new Writer(Application.dataPath + myDebugfileVerts);
            wrVerts.Initialize();
            wrVerts.WriteToFile("Vert_Idx;Position;\n");
            for (int i = 0; i < verts.Length; i++)
            {
                wrVerts.WriteToFile(i + ";" + verts[i].ToString() + ";\n");
            }

            Writer wrTriangles = new Writer(Application.dataPath + myDebugfileTriangles);
            wrTriangles.Initialize();
            wrTriangles.WriteToFile("Triangle_Idx;Vert_Idx;\n");
            for (int i = 0; i < triangles.Length; i++)
            {
                wrTriangles.WriteToFile(i + ";" + triangles[i].ToString() + ";\n");
            }

            Writer wrHE = new Writer(Application.dataPath + myDebugfileHE);
            wrHE.Initialize();
            wrHE.WriteToFile("HE_Idx;FaceIdx;initVert;finVert;nextEdge;twinEdge;\n");
            for (int i = 0; i < HE.Length; i++)
            {
                wrHE.WriteToFile(i + ";" + HE[i].faceIndex + ";" + HE[i].initialVertex + ";" + HE[i].finalVertex + ";" + HE[i].nextEdge + ";" + HE[i].twinEdge + ";\n");
            }

        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

    }

    private void writePerformance()
    {
        if (myDebugfilePerf == null)
        {
            myDebugfilePerf = "/Debug Unity/RunTimeMeshShattering_Perf.csv";
        }
        try
        {
            string myFilePath = Application.dataPath + myDebugfilePerf;
            Writer wrPerf = new Writer(myFilePath);
            if (!File.Exists(myFilePath))
            { 
                wrPerf.WriteToFile("Oggetto;Triangoli;Frammenti;Ms. HE;Ms. BFS;Ms. Explode\n");
            }
            string obj = this.gameObject.name;
            wrPerf.WriteToFile(obj + ";" + triangles.Length/3 + ";" + fragments + ";" +
                               elapsedHE + ";" + elapsedBFS + ";" + elapsedEX + ";\n");
            

        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

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
            gameObject.GetComponent<MeshShatterer>().ExplosionHandler(fragments, HE, fragmentsFaces);
            Debug.Log("Spacco!");
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        gameObject.GetComponent<MeshShatterer>().ExplosionHandler(fragments, HE, fragmentsFaces);
        watch.Stop();
        elapsedEX = watch.ElapsedMilliseconds;
        if (writeDebugPerformance)
        {
            writePerformance();
        }

    }
}

