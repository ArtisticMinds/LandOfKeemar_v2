using System.Collections.Generic;
using UnityEngine;

public class DecalBuilder
{
    private static readonly List<Vector3> bufVertices = new List<Vector3>();
    private static readonly List<Vector3> bufNormals = new List<Vector3>();
    private static readonly List<Vector2> bufTexCoords = new List<Vector2>();
    private static readonly List<int> bufIndices = new List<int>();


    public static void BuildDecalForObject(Decal decal, GameObject affectedObject)
    {
        if (Application.isPlaying) return;

        var affectedMesh = affectedObject.GetComponent<MeshFilter>().sharedMesh;
        if (affectedMesh == null) return;

        var maxAngle = decal.maxAngle;

        var right = new Plane(Vector3.right, Vector3.right / 2f);
        var left = new Plane(-Vector3.right, -Vector3.right / 2f);

        var top = new Plane(Vector3.up, Vector3.up / 2f);
        var bottom = new Plane(-Vector3.up, -Vector3.up / 2f);

        var front = new Plane(Vector3.forward, Vector3.forward / 2f);
        var back = new Plane(-Vector3.forward, -Vector3.forward / 2f);

        var vertices = affectedMesh.vertices;
        var triangles = affectedMesh.triangles;
        var startVertexCount = bufVertices.Count;

        var matrix = decal.transform.worldToLocalMatrix * affectedObject.transform.localToWorldMatrix;

        for (var i = 0; i < triangles.Length; i += 3)
        {
            var i1 = triangles[i];
            var i2 = triangles[i + 1];
            var i3 = triangles[i + 2];

            var v1 = matrix.MultiplyPoint(vertices[i1]);
            var v2 = matrix.MultiplyPoint(vertices[i2]);
            var v3 = matrix.MultiplyPoint(vertices[i3]);

            var side1 = v2 - v1;
            var side2 = v3 - v1;
            var normal = Vector3.Cross(side1, side2).normalized;

            if (Vector3.Angle(-Vector3.forward, normal) >= maxAngle) continue;


            var poly = new DecalPolygon(v1, v2, v3);

            poly = DecalPolygon.ClipPolygon(poly, right);
            if (poly == null) continue;
            poly = DecalPolygon.ClipPolygon(poly, left);
            if (poly == null) continue;

            poly = DecalPolygon.ClipPolygon(poly, top);
            if (poly == null) continue;
            poly = DecalPolygon.ClipPolygon(poly, bottom);
            if (poly == null) continue;

            poly = DecalPolygon.ClipPolygon(poly, front);
            if (poly == null) continue;
            poly = DecalPolygon.ClipPolygon(poly, back);
            if (poly == null) continue;

            AddPolygon(poly, normal);
        }

        if (decal.sprite)
        {
            GenerateTexCoords(startVertexCount, decal.sprite);
        }
        else
        {
            Sprite fakeSprite;
            var textureWidth = decal.material.mainTexture.width;
            var textureHeight = decal.material.mainTexture.height;
            var textureData = new Texture2D(textureWidth, textureHeight);
            fakeSprite = Sprite.Create(textureData, new Rect(0, 0, textureWidth, textureHeight),
                new Vector2(textureWidth / 2, textureHeight / 2));
            GenerateTexCoords(startVertexCount, decal.sprite);
        }
    }

    private static void AddPolygon(DecalPolygon poly, Vector3 normal)
    {
        var ind1 = AddVertex(poly.vertices[0], normal);
        for (var i = 1; i < poly.vertices.Count - 1; i++)
        {
            var ind2 = AddVertex(poly.vertices[i], normal);
            var ind3 = AddVertex(poly.vertices[i + 1], normal);

            bufIndices.Add(ind1);
            bufIndices.Add(ind2);
            bufIndices.Add(ind3);
        }
    }

    private static int AddVertex(Vector3 vertex, Vector3 normal)
    {
        var index = FindVertex(vertex);
        if (index == -1)
        {
            bufVertices.Add(vertex);
            bufNormals.Add(normal);
            index = bufVertices.Count - 1;
        }
        else
        {
            var t = bufNormals[index] + normal;
            bufNormals[index] = t.normalized;
        }

        return index;
    }

    private static int FindVertex(Vector3 vertex)
    {
        for (var i = 0; i < bufVertices.Count; i++)
            if (Vector3.Distance(bufVertices[i], vertex) < 0.01f)
                return i;
        return -1;
    }

    private static void GenerateTexCoords(int start, Sprite sprite)
    {
        var rect = sprite.rect;
        rect.x /= sprite.texture.width;
        rect.y /= sprite.texture.height;
        rect.width /= sprite.texture.width;
        rect.height /= sprite.texture.height;

        for (var i = start; i < bufVertices.Count; i++)
        {
            var vertex = bufVertices[i];

            var uv = new Vector2(vertex.x + 0.5f, vertex.y + 0.5f);
            uv.x = Mathf.Lerp(rect.xMin, rect.xMax, uv.x);
            uv.y = Mathf.Lerp(rect.yMin, rect.yMax, uv.y);

            bufTexCoords.Add(uv);
        }
    }

    public static void Push(float distance)
    {
        for (var i = 0; i < bufVertices.Count; i++)
        {
            var normal = bufNormals[i];
            bufVertices[i] += normal * distance;
        }
    }


    public static Mesh CreateMesh()
    {
        if (bufIndices.Count == 0) return null;
        var mesh = new Mesh();

        mesh.vertices = bufVertices.ToArray();
        mesh.normals = bufNormals.ToArray();
        mesh.uv = bufTexCoords.ToArray();
        mesh.uv2 = bufTexCoords.ToArray();
        mesh.triangles = bufIndices.ToArray();

        bufVertices.Clear();
        bufNormals.Clear();
        bufTexCoords.Clear();
        bufIndices.Clear();

        return mesh;
    }
}