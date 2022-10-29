using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{

    private Vector3 pos;

    private void Update()
    {
        bool hitTouch = Input.touchCount == 1 || Input.GetMouseButtonDown(0);

        if (hitTouch)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.touches[0];
                pos = touch.position;
            }
            else
            {

                pos = Input.mousePosition;
            }

            Ray ray = Camera.main.ScreenPointToRay(pos);
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
    }
}
