using UnityEngine.Events;
using UnityEngine;

public class DraggingObject : MonoBehaviour
{
    private float dist;
    private bool dragging = false;
    private Transform toDrag;
    private float originalY;
    public bool dragActive;
    public Material selectedMaterial;
    [HideInInspector]
    public Material defaultMaterial;
    public float onDragScale=1.3F;
    public float distFormDrag;
    public Collider dreagArea;
    Vector3 minBounds; 
    Vector3 maxBounds;
    [Header("Evento ad inizio Drag")]
    public UnityEvent onStartDrag;

    [Header("Evento a fine Drag")]
    public UnityEvent onEndDrag;

    [Header("Evento a fine Drag sul punto di ancoraggio")]
    public UnityEvent onEndDragInPoint;

    void Start()
    {
        originalY = transform.position.y;
        defaultMaterial = GetComponent<Renderer>().material;
    }


    void DragEditor()
    {


        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
            GetComponent<Renderer>().material = defaultMaterial;
            return;
        }

        Vector3 v3;
        Vector3 pos = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
   
            Ray ray = InteractionManager.sceneCam.ScreenPointToRay(pos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                
                if (hit.collider != null)
                {
                    if (hit.collider.tag.Equals("InteractiveObject")&& hit.collider.GetComponent<DraggingObject>())
                    {
                        Debug.Log("Start Dragging");

                        
                        toDrag = hit.transform;
                        dist = hit.transform.position.z - InteractionManager.sceneCam.transform.position.z;
                        v3 = new Vector3(1-pos.x, 1-pos.y, dist);
                        v3 = InteractionManager.sceneCam.ScreenToWorldPoint(v3);
                        dragging = true;
                        onStartDrag.Invoke();


                    }
                }
            }



        }

        if (dragging)
        {

            GetComponent<Renderer>().material = selectedMaterial;
            toDrag.localScale = Vector3.one * onDragScale;
           // Debug.Log("Dragging" + toDrag.position.x);
            //X-0.3

            Ray r = InteractionManager.sceneCam.ScreenPointToRay(new Vector3( Input.mousePosition.x,  Input.mousePosition.y, dist));
            Debug.DrawRay(r.origin, r.direction * 10, Color.white);
            toDrag.position = Vector3.Lerp(toDrag.position, r.GetPoint(originalY), Time.deltaTime*5);


            minBounds = dreagArea.bounds.min;
            maxBounds = dreagArea.bounds.max;

            float xPos = Mathf.Clamp(toDrag.position.x, minBounds.x, maxBounds.x);
            float yPos = Mathf.Clamp(toDrag.position.y, minBounds.y, maxBounds.y);
            float zPos = Mathf.Clamp(toDrag.position.z, minBounds.z, maxBounds.z);

            toDrag.position = new Vector3(xPos, yPos, zPos);


            distFormDrag = Vector3.Distance(toDrag.position, dreagArea.transform.position);
           // Debug.Log(distFormDrag);

            if (distFormDrag > 1F)
            {
                StopDragging();
            }
        }


        if (dragging&&Input.GetMouseButtonUp(0))
        {
            StopDragging();
            return;
        }

       

    }

    void DragMobile()
    {

        if (!Input.touchSupported) return;

        if (Input.touchCount != 1)
        {
            StopDragging();
            return;
        }

        Vector3 v3;
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
                    if (hit.collider.tag.Equals("InteractiveObject") && hit.collider.GetComponent<DraggingObject>())
                    {
                      //  DebugConsole.Log("Start Dragging");

                        toDrag = hit.transform;
                        dist = hit.transform.position.z - InteractionManager.sceneCam.transform.position.z;
                        v3 = new Vector3(1 - pos.x, 1 - pos.y, dist);
                        v3 = InteractionManager.sceneCam.ScreenToWorldPoint(v3);
                        GetComponent<Renderer>().material = selectedMaterial;
                        dragging = true;


                    }
                }
            }



        }

        if (dragging && touch.phase == TouchPhase.Moved)
        {

            GetComponent<Renderer>().material = selectedMaterial;
            toDrag.localScale = Vector3.one * onDragScale;
            


            Ray r = InteractionManager.sceneCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, dist));
            Debug.DrawRay(r.origin, r.direction * 10, Color.white);
            distFormDrag = Vector3.Distance(toDrag.position, r.GetPoint(originalY));
            toDrag.position = Vector3.Lerp(toDrag.position, r.GetPoint(originalY), Time.deltaTime * 5);
        }

        Debug.Log(distFormDrag);
        if (distFormDrag > 1F)
        {
            StopDragging();

        }

        if (dragging && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
        {
            StopDragging();
        }
    }

    void StopDragging()
    {
        if (dragging)
        {
            Debug.Log("Stop Dragging");
            GetComponent<Renderer>().material = defaultMaterial;
            toDrag.localScale = Vector3.one;
            dragging = false;
            onEndDrag.Invoke();
            distFormDrag = 0;
        }
    }

    void Update()
    {

        if (!dragActive)
        {
            dragging = false;
            return;
        }

#if UNITY_EDITOR
        DragEditor();
#endif

#if UNITY_ANDROID

        DragMobile();
#endif

#if UNITY_IOS

        DragMobile();
#endif

        InteractionManager.camController.canRotate = !dragging;

        if(dragging)
        toDrag.position = new Vector3(toDrag.position.x, originalY, toDrag.position.z);
    }
}
