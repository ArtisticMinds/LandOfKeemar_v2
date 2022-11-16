using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager instance;
    private Vector3 pos;
    public static Camera sceneCam;
    public static CameraController camController;
    public static bool oneTouch;
    public static bool interactionActive = true;


    private void Awake()
    {
        if(!camController)
        camController = FindObjectOfType<CameraController>();

        if (!sceneCam)
        sceneCam = camController.GetComponent<Camera>();

        if (instance == null)
        {
            instance = this;
        }


    }

    //Chiamata dai pulsanti
    public void SceneObjectsInteractions(bool activate)
    {
        interactionActive = activate;
    }
    

    private void Update()
    {

        if (!interactionActive) return;

        if (Input.touchCount>0)
        {
            if (Input.touches[0].phase == TouchPhase.Ended) oneTouch = false;
            if (Input.touches[0].phase == TouchPhase.Moved) oneTouch = true;
        }


        if (Input.touchCount==1 && Input.touches[0].phase== TouchPhase.Began && !oneTouch)
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
                        oneTouch = true;
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
