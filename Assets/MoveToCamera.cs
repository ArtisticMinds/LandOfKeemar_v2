using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToCamera : MonoBehaviour
{
    [SerializeField] Vector3 offset;
    [SerializeField] float speed = 3;
    [SerializeField] bool active;

   public void Activate()
    {
        active=true;
    }
    public void Deactivate()
    {
        active=false;
    }
    public void DeactivateAndDestroy(float dealay)
    {
        active = true;
       Destroy(gameObject,dealay);
    }


    void LateUpdate()
    {
        if (active)
        {
            transform.SetParent(InteractionManager.camController.collectObjectPoint);
            transform.position = Vector3.Slerp(transform.position, (InteractionManager.camController.collectObjectPoint.position + offset * InteractionManager.sceneCam.fieldOfView/12)+ (InteractionManager.camController.collectObjectPoint.right * InteractionManager.sceneCam.fieldOfView / 150), Time.deltaTime * speed);
        }
    }
}
