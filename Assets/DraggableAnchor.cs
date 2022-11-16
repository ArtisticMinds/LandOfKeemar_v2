using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public class DraggableAnchor : MonoBehaviour
{

    [Header("Evento all'avvenuta connessione con il draggable")]
    public UnityEvent OnConnect;


    private void Start()
    {

    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow cube at the transform position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2F);
    }

    private void OnTriggerEnter(Collider other)
    {
        DraggingObject draggable = other.gameObject.GetComponent<DraggingObject>();
        if (draggable && other.tag.Equals("InteractiveObject"))
        {
            Connect(draggable);
        }
    }

    public void Connect(DraggingObject draggable)
    {
        OnConnect.Invoke();
        draggable.dragActive = false;
        draggable.transform.position = transform.position;
        draggable.transform.rotation = transform.rotation;
        draggable.GetComponent<Rigidbody>().isKinematic = true;
        draggable.GetComponent<Collider>().isTrigger = true;
        InteractionManager.camController.canRotate = true;
        draggable.GetComponent<Renderer>().material = draggable.defaultMaterial;
        draggable.transform.localScale = Vector3.one;
        draggable.onEndDragInPoint.Invoke();
        //DebugConsole.Log("Connect Dragging");
    }
}
