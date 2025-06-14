using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Decal : MonoBehaviour
{
    public Material material;
    public Sprite sprite;

    public float maxAngle = 90.0f;
    public float pushDistance = 0.009f;
    public LayerMask affectedLayers = -1;
    private GameObject[] affectedObjects;

    private Matrix4x4 oldMatrix;
    private Vector3 oldScale;

    // Update is called once per frame
    private void Update()
    {
        // Only rebuild mesh when scaling
        //bool hasChanged = oldMatrix != transform.localToWorldMatrix;
        var hasChanged = oldScale != transform.localScale;
        //oldMatrix = transform.localToWorldMatrix;
        oldScale = transform.localScale;


        if (hasChanged) BuildDecal(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    public Bounds GetBounds()
    {
        var size = transform.lossyScale;
        var min = -size / 2f;
        var max = size / 2f;

        Vector3[] vts =
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(max.x, max.y, min.z),

            new Vector3(min.x, min.y, max.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, max.y, max.z)
        };

        for (var i = 0; i < 8; i++) vts[i] = transform.TransformDirection(vts[i]);

        min = max = vts[0];
        foreach (var v in vts)
        {
            min = Vector3.Min(min, v);
            max = Vector3.Max(max, v);
        }

        return new Bounds(transform.position, max - min);
    }

    public void BuildDecal(Decal decal)
    {
        var filter = decal.GetComponent<MeshFilter>();
        if (filter == null) filter = decal.gameObject.AddComponent<MeshFilter>();
        if (decal.GetComponent<Renderer>() == null) decal.gameObject.AddComponent<MeshRenderer>();
        decal.material = decal.GetComponent<Renderer>().material;

        if (decal.material == null || decal.sprite == null)
        {
            filter.mesh = null;
            return;
        }


        affectedObjects = GetAffectedObjects(decal.GetBounds(), decal.affectedLayers);
        foreach (var go in affectedObjects) DecalBuilder.BuildDecalForObject(decal, go);
        DecalBuilder.Push(decal.pushDistance);

        var mesh = DecalBuilder.CreateMesh();
        if (mesh != null)
        {
            mesh.name = "DecalMesh";
            filter.mesh = mesh;
        }
    }

    private static bool IsLayerContains(LayerMask mask, int layer)
    {
        //Debug.Log("Mask value is " + mask.value);
        //Debug.Log("Layer value is " + (layer >> 2));
        if (mask.value >= 0)
            return ((mask.value >> 2) & layer) != 0;
        return true;
    }

    private static GameObject[] GetAffectedObjects(Bounds bounds, LayerMask affectedLayers)
    {
        var renderers = FindObjectsOfType<MeshRenderer>();
        var objects = new List<GameObject>();
        foreach (Renderer r in renderers)
        {
            if (!r.enabled) continue;
            /*
            if (r.gameObject.name == "bonnet") {
                Debug.Log("bonnet layer is " + r.gameObject.layer);
                //int test = (affectedLayers.value >> 2);
                Debug.Log("affected layer is " + (affectedLayers.value >> 2));

                Debug.Log("Mask test: " + (affectedLayers.value & r.gameObject.layer >> 2));
                //Debug.Log("Mask test: " + (r.gameObject.layer & (affectedLayers.value >> 2)));
            }
            */
            if (!IsLayerContains(affectedLayers, r.gameObject.layer)) continue;
            if (r.GetComponent<Decal>() != null) continue;

            if (bounds.Intersects(r.bounds)) objects.Add(r.gameObject);
        }

        return objects.ToArray();
    }
}