using System.Collections.Generic;
using UnityEngine;

public class DecalPolygon
{
    public List<Vector3> vertices = new List<Vector3>(9);

    public DecalPolygon(params Vector3[] vts)
    {
        vertices.AddRange(vts);
    }

    public static DecalPolygon ClipPolygon(DecalPolygon polygon, Plane plane)
    {
        var positive = new bool[9];
        var positiveCount = 0;

        for (var i = 0; i < polygon.vertices.Count; i++)
        {
            positive[i] = !plane.GetSide(polygon.vertices[i]);
            if (positive[i]) positiveCount++;
        }

        if (positiveCount == 0) return null; // полностью за плоскостью
        if (positiveCount == polygon.vertices.Count) return polygon; // полностью перед плоскостью

        var tempPolygon = new DecalPolygon();

        for (var i = 0; i < polygon.vertices.Count; i++)
        {
            var next = i + 1;
            next %= polygon.vertices.Count;

            if (positive[i]) tempPolygon.vertices.Add(polygon.vertices[i]);

            if (positive[i] != positive[next])
            {
                var v1 = polygon.vertices[next];
                var v2 = polygon.vertices[i];

                var v = LineCast(plane, v1, v2);
                tempPolygon.vertices.Add(v);
            }
        }

        return tempPolygon;
    }

    private static Vector3 LineCast(Plane plane, Vector3 a, Vector3 b)
    {
        float dis;
        var ray = new Ray(a, b - a);
        plane.Raycast(ray, out dis);
        return ray.GetPoint(dis);
    }
}