using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCamera : MonoBehaviour
{

    Camera cam;
    float _rotationY;
    float _rotationX;
    float clickXPosition;
    float clickYPosition;
    float mouseY;
    float mouseX;
    float zoom=1;
    float deltaDistance;
    float oldTouchDistance;
    float currentTouchDistance;
    float zoomPower;
    float scroll;

    [SerializeField]
    float sensitivy;


    [SerializeField]
    float maxZoom = 30;
    [SerializeField]
    float minZoom = 80;

    [SerializeField]
    private float _minAngle = -20;
    [SerializeField]
    private float _maxAngle = 80;
    [SerializeField]
    private float clampMovement = 5; 
    [SerializeField]
   Transform _target;

    [SerializeField]
    private float _distanceFormTarget=3;

    private Vector3 _currentRotation;
    private Vector3 _smootVelocity = Vector3.zero;
    Vector3 nextRotation;
    [SerializeField]
    private float _smootTime = 3;
    void Start()
    {
        cam = GetComponent<Camera>();
        transform.position = _target.position - transform.forward * _distanceFormTarget;
    }



    // Update is called once per frame
    void Update()
    {


        if ((Input.touchCount == 1 &&
            Input.GetTouch(0).phase != TouchPhase.Moved) ) 
        {

            clickXPosition = Input.GetAxis("Mouse X");
            clickYPosition = Input.GetAxis("Mouse Y");
        }
        else {
            if ((Input.touchCount == 1  &&
      Input.GetTouch(0).phase == TouchPhase.Moved) )
            {
                mouseX = (Input.GetAxis("Mouse X") - clickXPosition) * sensitivy;
                mouseY = -(Input.GetAxis("Mouse Y") - clickYPosition) * sensitivy;

                _rotationY += mouseX;
                _rotationX += mouseY;

                
            }
            

        }


            if (Input.touchCount == 2)
            {

                // get current touch positions
                Touch tZero = Input.GetTouch(0);
                Touch tOne = Input.GetTouch(1);
                // get touch position from the previous frame
                Vector2 tZeroPrevious = tZero.position - tZero.deltaPosition;
                Vector2 tOnePrevious = tOne.position - tOne.deltaPosition;

                 oldTouchDistance = Vector2.Distance(tZeroPrevious, tOnePrevious);
                 currentTouchDistance = Vector2.Distance(tZero.position, tOne.position);


            zoomPower = oldTouchDistance - currentTouchDistance;
        }
        else
        {
            if (zoomPower > 0)
                zoomPower -= Time.deltaTime;
            else
                zoomPower = 0;
        }



        _rotationX = Mathf.Clamp(_rotationX, _minAngle, _maxAngle);
        nextRotation = new Vector3(_rotationX, _rotationY);
        _currentRotation = Vector3.SmoothDamp(_currentRotation, nextRotation, ref _smootVelocity, _smootTime);
        transform.localEulerAngles = _currentRotation;

        Zoom(Mathf.Lerp(deltaDistance, zoomPower, _smootTime * 0.3F), _smootTime * 0.5F);

        moveOrizontal += scroll * Time.deltaTime;
        moveOrizontal = Mathf.Clamp(moveOrizontal, -clampMovement, clampMovement);
        scSpeed = Mathf.Lerp(scSpeed, moveOrizontal, Time.deltaTime);

        transform.position = Vector3.Lerp(transform.position, (_target.position - transform.forward * _distanceFormTarget)+transform.right* scSpeed, _smootTime * 0.5F);


    }

    void Zoom(float deltaMagnitudeDiff, float speed)
    {

        cam.fieldOfView += deltaMagnitudeDiff * speed;
        // set min and max value of Clamp function upon your requirement
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minZoom, maxZoom);
    }
    public float moveOrizontal;
    float scSpeed;
    public void Scroll(float direction)
    {
   
        scroll = direction;

    }
    public void EndScroll()
    {
        scroll = 0;
        
    }
}
