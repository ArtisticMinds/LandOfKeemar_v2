using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    public LayerMask collisionLayers;
    public static bool collision;
    public static bool triggerStay;

    // Start is called before the first frame update
    private void Awake()
    {


    }

    // Update is called once per frame
    void Update()
    {
        CamCollision();
    }


    private void OnTriggerStay(Collider other)
    {
        triggerStay = true;
    }

    private void OnTriggerExit(Collider other)
    {
        triggerStay = false;
    }


    public void CamCollision()
    {

        //Ray r = new Ray(transform.position, transform.right);
        //Debug.DrawRay(r.origin, r.direction, Color.red);
        //Ray r2 = new Ray(transform.position, -transform.right);
        //Debug.DrawRay(r2.origin, r2.direction, Color.red);
        Ray r3 = new Ray(transform.position, -transform.up);
        Debug.DrawRay(r3.origin, r3.direction, Color.red);
        //Ray r4 = new Ray(transform.position, transform.up);
        //Debug.DrawRay(r4.origin, r4.direction, Color.red);




       // if (Physics.Linecast(transform.parent.position, deisredCameraPos, out hit))
        if (Physics.Raycast(r3,1F, collisionLayers)|| triggerStay)
        {
            collision = true;
            GetComponent<CameraController>().moveVertical = 0.1f;

        }
        else
        {
            if(collision)
            GetComponent<CameraController>().moveVertical = 0.02F;
              collision = false;
        }


    }
}
