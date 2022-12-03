using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public class DraggableAnchor : MonoBehaviour
{

    [Header("Evento all'avvenuta connessione con il draggable")]
    public UnityEvent OnConnect;



    void OnDrawGizmosSelected()
    {
        // Draw a yellow cube at the transform position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2F);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("DraggableObject"))
        {
            Connect(other.gameObject.GetComponent<DraggingObject>());
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
        draggable.renderer.material = draggable.defaultMaterial;
        draggable.transform.localScale =Vector3.one* draggable.originalScale;
        draggable.onEndDragInPoint.Invoke();
        DebugConsole.Log("Connect Dragging");
    }
}
