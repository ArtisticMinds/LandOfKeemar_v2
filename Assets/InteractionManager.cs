using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{

    private Vector3 pos;
    public static Camera sceneCam;

    private void Awake()
    {
        if(!sceneCam)
        sceneCam = FindObjectOfType<CameraController>().GetComponent<Camera>();
    }

    private void Update()
    {




        if(Input.touchCount>0 && Input.touches[0].phase== TouchPhase.Began)
        {
            Ray ray = sceneCam.ScreenPointToRay(Input.touches[0].position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    if (hit.collider.tag.Equals("InteractiveObject"))
                    {
                        MissionObject obj = hit.collider.gameObject.GetComponent<MissionObject>();
                        obj.OnTouchActivation();
                    }
                }
            }
        }

#if UNITY_EDITOR

        if (Input.GetMouseButtonDown(0))
        {

           pos = Input.mousePosition;

            Ray ray = sceneCam.ScreenPointToRay(pos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag.Equals("InteractiveObject"))
                {
                    MissionObject obj = hit.collider.gameObject.GetComponent<MissionObject>();
                    obj.OnTouchActivation();
                }
            }





        }
        #endif
    }

}
