using UnityEngine.Events;
using UnityEngine;
using System.Collections;

public class RotableObject : MissionObject
{
    public float Yrotation=-45;
    public float mouseRotateSpeed = 5f;
    public float touchRotateSpeed = 2;

    private Vector2 swipeDirection; //swipe delta vector2
    private bool underRotation = false;
    private Quaternion targetRot;
    [Header("Evento ad inizio Rotazione")]
    public UnityEvent onStarRotation;

    [Header("Evento a fine Rotazione")]
    public UnityEvent onEndRotation;

    [Header("Evento a fine Rotazione dopo delayTime")]
    public UnityEvent onEndRotationDelayed;
    public float delayTime;



    public void RotationEditor()
    {
        if (!InteractionManager.interactionActive) return;

        if (Input.GetMouseButtonUp(0))
        {
            StopRotation();
            underRotation = false;
    
            return;
        }


        Vector3 pos = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = InteractionManager.sceneCam.ScreenPointToRay(pos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {

                if (hit.collider != null)
                {
                    if (hit.collider.transform.Equals(transform) && hit.collider.tag.Equals("InteractiveObject"))
                    {
                        Debug.Log("Start Rotation");
                        InteractionManager.camController.canRotate = false;
                        Yrotation = InteractionManager.sceneCam.transform.rotation.eulerAngles.y-180;
                        underRotation = true;
                        onStarRotation.Invoke();
                       

                    }
                   
                }
            }



        }

        if (underRotation)
        {
            Yrotation += Input.GetAxis("Mouse X") * mouseRotateSpeed;

            Vector3 tempV = new Vector3(transform.eulerAngles.x, Yrotation, transform.eulerAngles.z);
            targetRot = Quaternion.Euler(tempV); //We are setting the rotation around X, Y, Z axis respectively
                                                 //Rotate Camera
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.smoothDeltaTime * 2 * 50);

        }



    }


    public void RotationMobile()
    {

        if (!Input.touchSupported) return;
        if (!InteractionManager.interactionActive) return;

        if (Input.touchCount != 1)
        {
            StopRotation();
            return;
        }

 
        Touch touch = Input.touches[0];
        Vector3 pos = touch.position;
        if (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began)
        {

            Ray ray = InteractionManager.sceneCam.ScreenPointToRay(Input.touches[0].position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    if (hit.collider.transform.Equals(transform) && hit.collider.tag.Equals("InteractiveObject"))
                    {
                        DebugConsole.Log("Start Rotation");
                        InteractionManager.camController.canRotate = false;
                        Yrotation = InteractionManager.sceneCam.transform.rotation.eulerAngles.y - 180;
                        onStarRotation.Invoke();
                        underRotation = true;

                    }
                }
            }



        }

        if (underRotation && touch.phase == TouchPhase.Moved)
        {
            swipeDirection += -touch.deltaPosition * touchRotateSpeed; //-1 make rotate direction natural

            targetRot = Quaternion.Euler(0, swipeDirection.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.smoothDeltaTime * 2 * 50);


        }


        if (underRotation && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
        {
            StopRotation();
            
        }
    }

     IEnumerator OnEndRotationDelayed()
    {
        yield return new WaitForSeconds(delayTime);
        onEndRotationDelayed.Invoke();
    }

    public void StopRotation()
    {
        if (underRotation)
        {
            Debug.Log("Stop Rotation");

            underRotation = false;
            onEndRotation.Invoke();
            StartCoroutine(OnEndRotationDelayed());
            InteractionManager.camController.canRotate = true;
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        RotationEditor();
#endif

#if UNITY_ANDROID

        RotationMobile();
#endif

#if UNITY_IOS

        RotationMobile();
#endif

   
    
    }
    
}
