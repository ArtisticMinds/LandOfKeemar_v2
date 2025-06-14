using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Decal))]
public class DecalEditor : Editor
{
    private static bool showAffectedObject;
    private GameObject[] affectedObjects;

    private List<Material> materials;

    private Matrix4x4 oldMatrix;
    private Vector3 oldScale;


    private void OnEnable()
    {
        var decals = (Decal[])FindObjectsOfType(typeof(Decal));
        materials = new List<Material>();
        foreach (var decal in decals)
            if (decal.material != null && !materials.Contains(decal.material))
                materials.Add(decal.material);
    }


    private void OnSceneGUI()
    {
        var decal = (Decal)target;

        if (Event.current.control) HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        if (Event.current.control && Event.current.type == EventType.MouseDown)
        {
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, 50))
            {
                decal.transform.position = hit.point;
                decal.transform.forward = -hit.normal;
            }
        }

        var scale = decal.transform.localScale;
        if (decal.sprite != null)
        {
            var ratio = decal.sprite.rect.width / decal.sprite.rect.height;
            if (oldScale.x != scale.x)
                scale.y = scale.x / ratio;
            else if (oldScale.y != scale.y)
                scale.x = scale.y * ratio;
            else if (scale.x != scale.y * ratio) scale.x = scale.y * ratio;
            decal.transform.localScale = scale;
        }

        var hasChanged = oldMatrix != decal.transform.localToWorldMatrix;
        oldMatrix = decal.transform.localToWorldMatrix;
        oldScale = decal.transform.localScale;


        if (hasChanged) BuildDecal(decal);
    }

    public override void OnInspectorGUI()
    {
        var decal = (Decal)target;

        decal.material = DrawMaterialList(decal.material, materials);
        decal.material = AssetField("Material", decal.material);
        if (decal.material != null && !materials.Contains(decal.material)) materials.Add(decal.material);
        EditorGUILayout.Separator();

        if (decal.material != null && decal.material.mainTexture != null)
        {
            decal.sprite = DrawSpriteList(decal.sprite, decal.material.mainTexture);
            if (decal.sprite && decal.sprite.texture != decal.material.mainTexture) decal.sprite = null;
        }

        EditorGUILayout.Separator();

        decal.maxAngle = EditorGUILayout.FloatField("Max Angle", decal.maxAngle);
        decal.maxAngle = Mathf.Clamp(decal.maxAngle, 1, 180);

        decal.pushDistance = EditorGUILayout.FloatField("Push Distance", decal.pushDistance);
        decal.pushDistance = Mathf.Clamp(decal.pushDistance, 0.01f, 1);
        EditorGUILayout.Separator();

        decal.affectedLayers = LayerMaskField("Affected Layers", decal.affectedLayers);

        showAffectedObject = EditorGUILayout.Foldout(showAffectedObject, "Affected Objects");
        if (showAffectedObject && affectedObjects != null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginVertical();
            foreach (var go in affectedObjects) EditorGUILayout.ObjectField(go, typeof(GameObject), true);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Separator();

        GUILayout.Box("Left Ctrl + Left Mouse Button - Set position and normal of decal", GUILayout.ExpandWidth(true));

        if (GUI.changed) BuildDecal(decal);
    }

    private static T AssetField<T>(string label, T obj) where T : Object
    {
        return (T)EditorGUILayout.ObjectField(label, obj, typeof(T), false);
    }

    private static Material DrawMaterialList(Material material, List<Material> list)
    {
        var names = new string[list.Count];
        for (var i = 0; i < list.Count; i++) names[i] = list[i].name;

        var selected = list.IndexOf(material);
        selected = EditorGUILayout.Popup("Material", selected, names);
        if (selected != -1) return list[selected];
        return null;
    }

    private static Sprite DrawSpriteList(Sprite sprite, Texture texture)
    {
        var path = AssetDatabase.GetAssetPath(texture);
        var objs = AssetDatabase.LoadAllAssetsAtPath(path);
        var sprites = new List<Sprite>();
        foreach (var o in objs)
            if (o is Sprite)
                sprites.Add((Sprite)o);

        return DrawSpriteList(sprite, sprites.ToArray(), texture);
    }

    private static Sprite DrawSpriteList(Sprite sprite, Sprite[] list, Texture texture)
    {
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.MinHeight(50));
        for (int i = 0, y = 0; i < list.Length; y++)
        {
            GUILayout.BeginHorizontal();
            for (var x = 0; x < 5; x++, i++)
            {
                var rect = GUILayoutUtility.GetAspectRect(1);
                if (i < list.Length)
                {
                    var spr = list[i];
                    var selected = DrawItem(rect, spr, sprite == spr);
                    if (selected) sprite = spr;
                }
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        return sprite;
    }

    private static bool DrawItem(Rect rect, Sprite sprite, bool selected)
    {
        if (selected)
        {
            GUI.color = Color.blue;
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
            GUI.color = Color.white;
        }

        Texture texture = sprite.texture;
        var texRect = sprite.rect;
        texRect.x /= texture.width;
        texRect.y /= texture.height;
        texRect.width /= texture.width;
        texRect.height /= texture.height;
        GUI.DrawTextureWithTexCoords(rect, texture, texRect);

        selected = Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition);
        if (selected)
        {
            GUI.changed = true;
            Event.current.Use();
            return true;
        }

        return false;
    }

    private static LayerMask LayerMaskField(string label, LayerMask mask)
    {
        var layers = new List<string>();
        for (var i = 0; i < 32; i++)
        {
            var name = LayerMask.LayerToName(i);
            //Debug.Log("Layer name is " + name);
            if (name != "") layers.Add(name);
        }

        //for(int i=0; i<32; i++) {
        //Debug.Log("Selected masks are " + LayerMask.LayerToName(mask.value));
        //}

        return EditorGUILayout.MaskField(label, mask, layers.ToArray());
    }

    private static bool IsLayerContains(LayerMask mask, int layer)
    {
        //Debug.Log("Mask value is " + mask.value);
        //Debug.Log("Layer value is " + (layer >> 2));
        if (mask.value >= 0)
            return ((mask.value >> 2) & layer) != 0;
        return true;
    }


    private void BuildDecal(Decal decal)
    {
        var filter = decal.GetComponent<MeshFilter>();
        if (filter == null) filter = decal.gameObject.AddComponent<MeshFilter>();
        if (decal.GetComponent<Renderer>() == null) decal.gameObject.AddComponent<MeshRenderer>();
        decal.GetComponent<Renderer>().material = decal.material;

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