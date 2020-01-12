using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace aiuto
{

    // source: https://answers.unity.com/questions/1019436/get-outeredge-vertices-c.html
    public static class EdgeHelper
    {
        public struct Edge
        {
            public int v1; // indice del vertice 1 nell'array dei vertici 
            public int v2; // indice del vertice 2 nell'array dei vertici 
            public int triangleIndex; // indice di partenza nell'array dei triangoli

            // costruttore
            public Edge(int in_V1, int in_V2, int in_Index)
            //public Edge(int in_V1, int in_V2)
            {
                v1 = in_V1;
                v2 = in_V2;
                triangleIndex = in_Index;
            }
        }



        // accetta in input l'array dei triangoli e restituisce l'elenco degli spigoli
        public static List<Edge> GetEdges(int[] in_Triangles)
        {
            List<Edge> result = new List<Edge>();
            for (int i = 0; i < in_Triangles.Length; i += 3)
            {
                int v1 = in_Triangles[i];
                int v2 = in_Triangles[i + 1];
                int v3 = in_Triangles[i + 2];
                result.Add(new Edge(v1, v2, i));
                result.Add(new Edge(v2, v3, i));
                result.Add(new Edge(v3, v1, i));
                //result.Add(new Edge(v1, v2));
                //result.Add(new Edge(v2, v3));
                //result.Add(new Edge(v3, v1));

            }
            return result;
        }

        // rielabora l'elenco degli edge e trova quelli che non sono in comune con nessun altro. Nel mio poligono convesso in sostanza non dovrei averne
        public static List<Edge> FindBoundary(this List<Edge> in_Edges)
        {
            List<Edge> result = new List<Edge>(in_Edges);
            for (int i = result.Count - 1; i > 0; i--)
            {
                for (int n = i - 1; n >= 0; n--)
                {
                    if (result[i].v1 == result[n].v2 && result[i].v2 == result[n].v1)
                    {
                        // shared edge so remove both
                        result.RemoveAt(i);
                        result.RemoveAt(n);
                        i--;
                        break;
                    }
                }
            }
            return result;
        }

        // riordina l'elenco degli edge
        public static List<Edge> SortEdges(this List<Edge> in_Edges)
        {
            List<Edge> result = new List<Edge>(in_Edges);
            for (int i = 0; i < result.Count - 2; i++)
            {
                Edge E = result[i];
                for (int n = i + 1; n < result.Count; n++)
                {
                    Edge a = result[n];
                    if (E.v2 == a.v1)
                    {
                        // in this case they are already in order so just continue with the next one
                        if (n == i + 1)
                            break;
                        // if we found a match, swap them with the next one after "i"
                        result[n] = result[i + 1];
                        result[i + 1] = a;
                        break;
                    }
                }
            }
            return result;
        }
    }
}