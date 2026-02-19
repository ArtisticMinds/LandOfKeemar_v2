using UnityEngine;
using System.Collections;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager instance;
    private Vector3 pos;
    public static Camera sceneCam;
    public static CameraController camController;
    public static bool interactionActive = true;
    public CanvasGroup zoomMessage;
    public CanvasGroup tutorialMessage;
    public float timer;
    //Dopo quanta inattività appare il messaggio
    public float timeToShowMessage = 10;
    private void Awake()
    {
        if(!camController)
            camController = FindFirstObjectByType<CameraController>();

        if (!sceneCam && camController)
            sceneCam = camController.GetComponent<Camera>();

        if (instance == null)
        {
            instance = this;
        }
        zoomMessage.gameObject.SetActive(false);


        
    }

    private void Start()
    {
        Debug.Log("SCENE:" + InGameCanvas.tappaScene.name);

        if (InGameCanvas.tappaScene.name.Equals("RoccaVarano"))
        {
            Invoke("ShowZoomMessage", 10);
        }
    }

    //Chiamata dai pulsanti
    public void SceneObjectsInteractions(bool activate)
    {
        interactionActive = activate;

        // Assicura che la camera non ruoti quando disabilitiamo le interazioni UI (es. translate)
        if (camController == null)
            camController = FindFirstObjectByType<CameraController>();

        if (camController != null)
            camController.canRotate = activate;

        Debug.Log("SceneObjectsInteractions - canRotate:" + activate);
    }

    //Apre il fumetto "non sai cosa fare?"
    public void OpenTutorialMessage()
    {
        timer = 0;
        tutorialMessage.GetComponent<Animator>().SetTrigger("OpenFumetto");
        timeToShowMessage += 20;
    }


    private void Update()
    {
        if (tutorialMessage)
        {
            if(!InGameCanvas.instance.tutorialPanel.activeInHierarchy) //Tutorial iniziale aperto
                timer += Time.deltaTime;

            if (timer > timeToShowMessage)
                OpenTutorialMessage();
        }

    //    if (DebugConsole.text02)DebugConsole.text02.text = "InteractionActive: " + interactionActive;
        if (!interactionActive) return;


        if (Input.touchCount==1 && Input.touches[0].phase== TouchPhase.Ended )
        {

            StartCoroutine(DealyedTouch());
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

    IEnumerator DealyedTouch()
    {
        yield return new WaitForEndOfFrame();

        if (!interactionActive) yield break;


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
                    timer = 0;
                    timeToShowMessage += 20;
                }
            }
        }
    }

    void ShowZoomMessage()
    {
        zoomMessage.gameObject.SetActive(true);
    }
}
