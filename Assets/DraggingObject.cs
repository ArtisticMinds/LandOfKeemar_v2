using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggingObject : MonoBehaviour
{
    private float dist;
    private bool dragging = false;
    private Vector3 offset;
    private Transform toDrag;
    void Start()
    {

    }


    void Update()
    {

        Vector3 v3;
        if (Input.touchCount != 1||Input.GetMouseButton(0)) 
        { 
            dragging = false;
            return; 
        }
        Touch touch = Input.touches[0];
        Vector3 pos = touch.position;

        if(Input.GetMouseButton(0)||(Input.touchCount > 0 && touch.phase == TouchPhase.Began))
        {
            Ray ray = InteractionManager.sceneCam.ScreenPointToRay(Input.touches[0].position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    if (hit.collider.tag.Equals("InteractiveObject"))
                    {
                        Debug.Log("Start Dragging");

                        toDrag = hit.transform;
                        dist = hit.transform.position.z - InteractionManager.sceneCam.transform.position.z;
                        v3 = new Vector3(pos.x, pos.y, dist);
                        v3 = InteractionManager.sceneCam.ScreenToWorldPoint(v3);
                        offset = toDrag.position - v3;
                        dragging = true;


                    }
                }
            }

  
                    
         }

        if (dragging && touch.phase == TouchPhase.Moved)
        {
            v3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, dist);
            v3 = InteractionManager.sceneCam.ScreenToWorldPoint(v3);
            toDrag.position = v3 + offset;

            Debug.Log("Dragging");
        }

        if (dragging && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
        {
            Debug.Log("Stop Dragging");
            dragging = false;
        }



    }
}
