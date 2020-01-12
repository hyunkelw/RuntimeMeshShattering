using System.Collections.Generic;
using UnityEngine;

public static class HalfEdgeHelper
{

    public struct HalfEdge
    {
        public int initialVertex; // indice del vertice di partenza nell'array dei vertici 
        public int finalVertex;   // indice del vertice di arrivo nell'array dei vertici 
        public int nextEdge;      // indice del prossimo edge nell'array degli edges 
        public int twinEdge;      // indice del prossimo edge nell'array degli edges 
        public int faceIndex;     // indice di partenza nell'array dei triangoli

        // costruttore
        public HalfEdge(int in_V1, int in_V2, int in_nE, int in_tE, int in_fI)
        {
            initialVertex = in_V1;
            finalVertex = in_V2;
            nextEdge = in_nE;
            twinEdge = in_tE;
            faceIndex = in_fI;
        }
    }

    public static HalfEdge[] GetHalfEdges(Vector3[] in_vertices, int[] in_triangles)
    {
        List<HalfEdge> halfEdges = new List<HalfEdge>();

        // si scorre tutto l'array dei triangoli
        for (int i = 0; i < in_triangles.Length; i += 3)
        {
            // per ogni triangolo, ci saranno 3 edge.
            // per i primi 2:
            // - Il vertice di partenza è l'indice di loop 
            // - Il vertice di arrivo è l'indice di loop + 1 
            // - il next edge è il prossimo elemento dell'array degli edge
            // - l'edge twin viene valorizzato in seguito
            // - la faccia di riferimento è l'indice di loop
            halfEdges.Add(new HalfEdge(in_triangles[i], in_triangles[i + 1], i + 1, 0, i));
            halfEdges.Add(new HalfEdge(in_triangles[i + 1], in_triangles[i + 2], i + 2, 0, i));

            // per l'ultimo edge invece
            // - il vertice di partenza è sempre l'indice di loop
            // - il vertice di arrivo è il primo della tripletta
            // - il next edge è il primo inserimento dei 3
            // - l'edge twin viene valorizzato in seguito
            // - la faccia di riferimento è sempre l'indice di loop
            halfEdges.Add(new HalfEdge(in_triangles[i + 2], in_triangles[i], i, 0, i));
        }

        HalfEdge[] HE = halfEdges.ToArray();

        // Nel secondo passaggio vengono valorizzati i twin edges        
        // si scorre tutto l'array degli half-edge creato finora
        for (int i = 0; i < HE.Length; i++)
        {
            // per ogni half-edge si cerca il gemello che abbia gli stessi vertici, ma in ordine inverso 
            // si parte dal fondo e si scorre all'indietro
            for (int j = HE.Length - 1; j > 0; j--)
            {
                // se il vertice iniziale del primo edge corrisponde al vertice finale del secondo edge 
                // e allo stesso tempo il vertice finale del primo edge corrisponde al vertice iniziale del secondo,
                // il gemello è stato trovato
                // non controllando gli indici ma direttamente il vettore mi assicuro di gestire anche i vertici duplicati
                if (V3Equal(in_vertices[HE[i].initialVertex], in_vertices[HE[j].finalVertex]) && V3Equal(in_vertices[HE[i].finalVertex], in_vertices[HE[j].initialVertex]))
                {
                    HE[i].twinEdge = j;
                    HE[j].twinEdge = i;
                    // una volta trovato il gemello si forza l'uscita dal ciclo interno
                    break;
                }
            }
        }
        return HE;
    }

    static bool V3Equal(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.0001;
    }
}

