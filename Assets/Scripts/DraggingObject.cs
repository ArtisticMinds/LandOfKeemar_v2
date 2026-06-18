using System.Collections.Generic;
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
    public Renderer rend;
    public float onDragScale=1.3F;
    [HideInInspector]
    public float originalScale;
    [HideInInspector]
    public float distFormDrag;
    public float maxDragDistance = 1;

    [Header("Area di drag (compatibile con singolo Collider o lista)")]
    [Tooltip("Vecchio campo singolo (compatibility). Se lasciato, usato come unica area se dragAreas è vuota.")]
    public Collider dreagArea;

    [Tooltip("Liste di collider che definiscono l'area permessa (unione). Se impostata, sostituisce dreagArea.")]
    public List<Collider> dragAreas = new List<Collider>();

    [Header("Aree proibite (buchi)")]
    [Tooltip("Collider all'interno dei quali NON è permesso posizionare l'oggetto.")]
    public List<Collider> forbiddenAreas = new List<Collider>();

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
        if (!rend)
            rend = GetComponent<Renderer>();

        originalScale = transform.localScale.x;
    }
    void Start()
    {
        originalY = transform.position.y;
        defaultMaterial = rend.material;
        startPosition = transform.position;
    }

    // ritorna true se point è considerato all'interno del collider (tolleranza)
    bool IsPointInsideCollider(Collider col, Vector3 point)
    {
        if (col == null) return false;
        Vector3 closest = col.ClosestPoint(point);
        // se ClosestPoint ritorna esattamente point significa che il punto è dentro il collider
        return (closest - point).sqrMagnitude < 1e-6f;
    }

    // trova il punto più vicino valido all'interno dell'unione delle dragAreas (o dreagArea come fallback)
    Vector3 GetNearestPointInAllowedAreas(Vector3 targetPos)
    {
        // Se è presente lista dragAreas e non vuota -> usa quella
        if (dragAreas != null && dragAreas.Count > 0)
        {
            Vector3 best = targetPos;
            float bestDistSq = float.MaxValue;
            foreach (var c in dragAreas)
            {
                if (c == null) continue;
                Vector3 cand = c.ClosestPoint(targetPos);
                float d2 = (cand - targetPos).sqrMagnitude;
                if (d2 < bestDistSq)
                {
                    bestDistSq = d2;
                    best = cand;
                }
            }
            return best;
        }

        // fallback al vecchio singolo collider
        if (dreagArea != null)
        {
            return dreagArea.ClosestPoint(targetPos);
        }

        // nessuna area definita => ritorna posizione target non modificata
        return targetPos;
    }

    // Applica i vincoli: inside allowed union e non dentro forbiddenAreas.
    Vector3 GetConstrainedPosition(Vector3 targetPos)
    {
        Vector3 constrained = GetNearestPointInAllowedAreas(targetPos);

        // Se nessuna forbidden definita -> return
        if (forbiddenAreas == null || forbiddenAreas.Count == 0)
            return constrained;

        // Se constrained è dentro un'area proibita, cerchiamo di "spostarlo" verso la superficie esterna più vicina,
        // quindi lo ri-proiettiamo dentro le allowed areas. Limitiamo i tentativi per evitare loop.
        const int maxAttempts = 6;
        int attempt = 0;
        while (attempt < maxAttempts)
        {
            bool insideAny = false;
            foreach (var forb in forbiddenAreas)
            {
                if (forb == null) continue;
                if (IsPointInsideCollider(forb, constrained))
                {
                    insideAny = true;

                    // costruiamo un punto di sample lontano nella direzione dal centro del forbidden verso constrained
                    Vector3 center = forb.bounds.center;
                    Vector3 dir = constrained - center;
                    if (dir.sqrMagnitude < 1e-6f) dir = Vector3.up; // fallback direzione
                    Vector3 sample = constrained + dir.normalized * 10f;

                    // ClosestPoint sul forbidden con sample esterno ci darà un punto sulla superficie verso l'esterno
                    Vector3 surface = forb.ClosestPoint(sample);

                    // ora ricalcoliamo il punto più vicino valido nelle allowed rispetto alla superficie dell'area proibita
                    constrained = GetNearestPointInAllowedAreas(surface);

                    break; // riesamina tutte le forbidden dalla nuova posizione
                }
            }

            if (!insideAny)
                break;

            attempt++;
        }

        return constrained;
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
                      //  DebugConsole.Log("Start Dragging");
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

            rend.material = selectedMaterial;
            toDrag.localScale = Vector3.one * onDragScale;
            // Debug.Log("Dragging" + toDrag.position.x);


            Ray r = InteractionManager.sceneCam.ScreenPointToRay(new Vector3(Input.mousePosition.x,  Input.mousePosition.y + dragOffset, dist));
            Debug.DrawRay(r.origin, r.direction * dist, Color.white);

            // target position before constraint
            Vector3 targetPos = Vector3.Lerp(toDrag.position, r.GetPoint(dist), Time.deltaTime*5);

            if ((dragAreas != null && dragAreas.Count > 0) || dreagArea != null)
            {
                Vector3 constrained = GetConstrainedPosition(targetPos);

                // preserve Y if needed
                if (freezeYposition)
                    constrained.y = originalY;

                toDrag.position = constrained;

                // update distance from center (optional) - use first available allowed or dreagArea
                if (dragAreas != null && dragAreas.Count > 0 && dragAreas[0] != null)
                    distFormDrag = Vector3.Distance(toDrag.position, dragAreas[0].transform.position);
                else if (dreagArea != null)
                    distFormDrag = Vector3.Distance(toDrag.position, dreagArea.transform.position);
            }
            else
            {
                if (freezeYposition)
                    targetPos.y = originalY;

                toDrag.position = targetPos;
            }

        }



    }

    public void DragMobile()
    {

        if (!Input.touchSupported) return;

     //-   if(dragging)
      //  DebugConsole.text01.text ="Dragging: "+ dragging+" "+transform.name;


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
                //        DebugConsole.Log("Start Dragging");
                        InteractionManager.camController.canRotate = false;

                        toDrag = hit.transform;
                        dist = Vector3.Distance(hit.transform.position, InteractionManager.sceneCam.transform.position);
                        v3 = new Vector3(1 - pos.x, 1 - pos.y, dist);
                        v3 = InteractionManager.sceneCam.ScreenToWorldPoint(v3);
                        rend.material = selectedMaterial;
                        dragging = true;


                    }
                }
            }



        }

        if (dragging && touch.phase == TouchPhase.Moved)
        {

            rend.material = selectedMaterial;
            toDrag.localScale = Vector3.one * onDragScale;
            


            Ray r = InteractionManager.sceneCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y+ dragOffset, dist));
            Debug.DrawRay(r.origin, r.direction * 10, Color.white);
            distFormDrag = Vector3.Distance(toDrag.position, r.GetPoint(dist));

            Vector3 targetPos = Vector3.Lerp(toDrag.position, r.GetPoint(dist), Time.deltaTime * 5);

            if ((dragAreas != null && dragAreas.Count > 0) || dreagArea != null)
            {
                Vector3 constrained = GetConstrainedPosition(targetPos);
                if (freezeYposition)
                    constrained.y = originalY;
                toDrag.position = constrained;
            }
            else
            {
                if (freezeYposition)
                    targetPos.y = originalY;
                toDrag.position = targetPos;
            }
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
         //   DebugConsole.Log("Stop Dragging");
            rend.material = defaultMaterial;
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
            if (dragging && toDrag != null)
                toDrag.position = new Vector3(toDrag.position.x, originalY, toDrag.position.z);
        }
    }
}
