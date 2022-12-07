using UnityEngine.Events;
using UnityEngine;

public class DraggingObject : MissionObject
{
    public float dragOffset;
    private float dist;
    private bool dragging = false;
    private Transform toDrag;
    private float originalY;
    public bool dragActive;
    public bool freezeYposition=true;
    public bool reposeOnEndDrag = false;
    private Vector3 startPosition;
    public Material selectedMaterial;
    [HideInInspector]
    public Material defaultMaterial;
    public Renderer renderer;
    public float onDragScale=1.3F;
    [HideInInspector]
    public float originalScale;
    [HideInInspector]
    public float distFormDrag;
    public float maxDragDistance = 1;
    public Collider dreagArea;
    Vector3 minBounds; 
    Vector3 maxBounds;
    [Header("Evento ad inizio Drag")]
    public UnityEvent onStartDrag;

    [Header("Evento a fine Drag")]
    public UnityEvent onEndDrag;

    [Header("Evento a fine Drag sul punto di ancoraggio")]
    public UnityEvent onEndDragInPoint;



    private void Awake()
    {
        if (!renderer)
            renderer = GetComponent<Renderer>();

        originalScale = transform.localScale.x;
    }
    void Start()
    {
        originalY = transform.position.y;
        defaultMaterial = renderer.material;
        startPosition = transform.position;
    }


   public void DragEditor()
    {

        if (Input.GetMouseButtonUp(0))
        {
            StopDragging();
            dragging = false;
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
                    if (hit.collider.transform.Equals(transform) && hit.collider.tag.Equals("DraggableObject"))
                    {
                        DebugConsole.Log("Start Dragging");
                        InteractionManager.camController.canRotate = false;

                        toDrag = hit.transform;
                        dist = Vector3.Distance(hit.transform.position,InteractionManager.sceneCam.transform.position);
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

            renderer.material = selectedMaterial;
            toDrag.localScale = Vector3.one * onDragScale;
            // Debug.Log("Dragging" + toDrag.position.x);


            Ray r = InteractionManager.sceneCam.ScreenPointToRay(new Vector3(Input.mousePosition.x,  Input.mousePosition.y + dragOffset, dist));
            Debug.DrawRay(r.origin, r.direction * dist, Color.white);
            toDrag.position = Vector3.Lerp(toDrag.position, r.GetPoint(dist), Time.deltaTime*5);

            if (dreagArea)
            {
                minBounds = dreagArea.bounds.min;
                maxBounds = dreagArea.bounds.max;
            }

            float xPos = Mathf.Clamp(toDrag.position.x, minBounds.x, maxBounds.x);
            float yPos = Mathf.Clamp(toDrag.position.y, minBounds.y, maxBounds.y);
            float zPos = Mathf.Clamp(toDrag.position.z, minBounds.z, maxBounds.z);

            toDrag.position = new Vector3(xPos, yPos, zPos);

            if (dreagArea)
            {
                distFormDrag = Vector3.Distance(toDrag.position, dreagArea.transform.position);


                if (distFormDrag > maxDragDistance)
                {
                    StopDragging();
                }
            }
        }



    }

    public void DragMobile()
    {

        if (!Input.touchSupported) return;

        if(dragging)
        DebugConsole.text01.text ="Dragging: "+ dragging+" "+transform.name;


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
                    if (hit.collider.transform.Equals(transform) && hit.collider.tag.Equals("DraggableObject") )
                    {
                        DebugConsole.Log("Start Dragging");
                        InteractionManager.camController.canRotate = false;

                        toDrag = hit.transform;
                        dist = Vector3.Distance(hit.transform.position, InteractionManager.sceneCam.transform.position);
                        v3 = new Vector3(1 - pos.x, 1 - pos.y, dist);
                        v3 = InteractionManager.sceneCam.ScreenToWorldPoint(v3);
                        renderer.material = selectedMaterial;
                        dragging = true;


                    }
                }
            }



        }

        if (dragging && touch.phase == TouchPhase.Moved)
        {

            renderer.material = selectedMaterial;
            toDrag.localScale = Vector3.one * onDragScale;
            


            Ray r = InteractionManager.sceneCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y+ dragOffset, dist));
            Debug.DrawRay(r.origin, r.direction * 10, Color.white);
            distFormDrag = Vector3.Distance(toDrag.position, r.GetPoint(dist));

       
            toDrag.position = Vector3.Lerp(toDrag.position, r.GetPoint(dist), Time.deltaTime * 5);
        }

      
        if (distFormDrag > maxDragDistance)
        {
            StopDragging();
        }

        if (dragging && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
        {
            StopDragging();
        }
    }

    public void StopDragging()
    {
        if (dragging)
        {
            DebugConsole.Log("Stop Dragging");
            renderer.material = defaultMaterial;
            toDrag.localScale = Vector3.one*originalScale;
            dragging = false;
            onEndDrag.Invoke();
            distFormDrag = 0;
            InteractionManager.camController.canRotate = true;

            if (reposeOnEndDrag)
            {
                transform.position = startPosition;
            }
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


        if (freezeYposition)
        {
            if (dragging)
                toDrag.position = new Vector3(toDrag.position.x, originalY, toDrag.position.z);
        }
    }
}
