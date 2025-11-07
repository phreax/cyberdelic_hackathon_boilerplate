using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Draws only the *unique* outer edges of a mesh (no diagonals).
/// Works great for cubes and low-poly objects.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
public class DrawUniqueEdges : MonoBehaviour
{
    public Color lineColor = Color.green;
    public float lineWidth = 2f;

    static Material lineMat;

    void OnEnable()
    {
        if (!lineMat)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMat = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            lineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMat.SetInt("_ZWrite", 0);
        }
    }

    void OnRenderObject()
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (!meshFilter || !meshFilter.sharedMesh) return;

        var mesh = meshFilter.sharedMesh;
        var verts = mesh.vertices;
        var tris = mesh.triangles;

        // Collect edges and count how many times each appears
        var edgeCount = new Dictionary<(int,int), int>();

        for (int i = 0; i < tris.Length; i += 3)
        {
            int a = tris[i];
            int b = tris[i + 1];
            int c = tris[i + 2];
            AddEdge(edgeCount, a, b);
            AddEdge(edgeCount, b, c);
            AddEdge(edgeCount, c, a);
        }

        lineMat.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.LINES);
        GL.Color(lineColor);

        // Draw only edges that appear once (outer edges)
        foreach (var kv in edgeCount)
        {
            if (kv.Value == 1)
            {
                var (i1, i2) = kv.Key;
                GL.Vertex(verts[i1]);
                GL.Vertex(verts[i2]);
            }
        }

        GL.End();
        GL.PopMatrix();
    }

    void AddEdge(Dictionary<(int,int), int> dict, int i1, int i2)
    {
        // Make sure edge direction doesn’t matter
        if (i1 > i2) (i1, i2) = (i2, i1);
        if (dict.ContainsKey((i1, i2))) dict[(i1, i2)]++;
        else dict[(i1, i2)] = 1;
    }
}
