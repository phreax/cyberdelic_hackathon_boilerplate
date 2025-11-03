using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class DrawCreaseEdges : MonoBehaviour
{
    public Color lineColor = Color.green;
    [Range(0, 180)] public float creaseAngle = 30f; // draw edges sharper than this
    public bool drawBoundaries = true;              // also draw open edges
    static Material lineMat;

    void OnEnable()
    {
        if (!lineMat)
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            lineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMat.SetInt("_ZWrite", 0);
        }
    }

    void OnRenderObject()
    {
        var mf = GetComponent<MeshFilter>();
        var mesh = mf ? mf.sharedMesh : null;
        if (!mesh) return;

        var verts = mesh.vertices;
        var tris  = mesh.triangles;

        // Build per-triangle normals
        int triCount = tris.Length / 3;
        var triNormals = new Vector3[triCount];
        for (int t = 0; t < triCount; t++)
        {
            int i0 = tris[t * 3 + 0], i1 = tris[t * 3 + 1], i2 = tris[t * 3 + 2];
            var p0 = verts[i0]; var p1 = verts[i1]; var p2 = verts[i2];
            triNormals[t] = Vector3.Cross(p1 - p0, p2 - p0).normalized;
        }

        // Map edges -> the triangle(s) that use them
        var edgeToTris = new Dictionary<(int,int), List<int>>(tris.Length);
        for (int t = 0; t < triCount; t++)
        {
            int i0 = tris[t * 3 + 0], i1 = tris[t * 3 + 1], i2 = tris[t * 3 + 2];
            AddEdge(edgeToTris, i0, i1, t);
            AddEdge(edgeToTris, i1, i2, t);
            AddEdge(edgeToTris, i2, i0, t);
        }

        float cosThresh = Mathf.Cos(creaseAngle * Mathf.Deg2Rad);

        lineMat.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.LINES);
        GL.Color(lineColor);

        foreach (var kv in edgeToTris)
        {
            var i1 = kv.Key.Item1; var i2 = kv.Key.Item2;
            var users = kv.Value;

            bool draw = false;
            if (users.Count == 1)
            {
                draw = drawBoundaries; // open edge
            }
            else if (users.Count == 2)
            {
                // crease test
                Vector3 n0 = triNormals[users[0]];
                Vector3 n1 = triNormals[users[1]];
                // draw if angle between normals > creaseAngle
                draw = Vector3.Dot(n0, n1) < cosThresh;
            }

            if (draw)
            {
                GL.Vertex(verts[i1]);
                GL.Vertex(verts[i2]);
            }
        }

        GL.End();
        GL.PopMatrix();
    }

    static void AddEdge(Dictionary<(int,int), List<int>> map, int a, int b, int tri)
    {
        if (a > b) (a, b) = (b, a); // undirected key
        if (!map.TryGetValue((a, b), out var list))
        {
            list = new List<int>(2);
            map[(a, b)] = list;
        }
        list.Add(tri);
    }
}
