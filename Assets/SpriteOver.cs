using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class SpriteOver : MonoBehaviour
{


    public GameObject target;
    public RectTransform canvasRect;
    [SerializeField]
    Vector2 offset;

    void Start()
    {
       // canvasRect = GetComponentInParent<RectTransform>();
        
    }


    void Update()
    {
        // Offset position above object bbox (in world space)
        float offsetPosY = target.transform.position.y + offset.y;
        float offsetPosX = target.transform.position.x + offset.x;

        // Final position of marker above GO in world space
        Vector3 offsetPos = new Vector3(offsetPosX, offsetPosY, target.transform.position.z);

        // Calculate *screen* position (note, not a canvas/recttransform position)
        Vector2 canvasPos;
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(offsetPos);

        // Convert screen position to Canvas / RectTransform space <- leave camera null if Screen Space Overlay
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out canvasPos);

        // Set
        transform.localPosition = canvasPos ;
    }
}
