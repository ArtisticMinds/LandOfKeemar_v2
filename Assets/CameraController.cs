using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //the center of the camera rotate sphere
    public Transform target;
    public Vector2 startRotation = new Vector2(-23, 20);
    public Camera sceneCamera;
    public Transform cameraParent;
    public Transform collectObjectPoint;
    [Range(5f, 15f)]
    [Tooltip("How sensitive the mouse drag to camera rotation")]
    public float mouseRotateSpeed = 5f;
    [Range(0.5f, 10f)]
    [Tooltip("How sensitive the touch drag to camera rotation")]
    public float touchRotateSpeed = 1f;
    [Tooltip("Smaller positive value means smoother rotation, 1 means no smooth apply")]
    public float rotationSmoothValue = 0.1f;
    [Tooltip("How long the smoothDamp of the mouse scroll takes")]
    public float zoomSmoothTime = 0.3f;
    public float editorFOVSensitivity = 5f;
    public float touchFOVSensitivity = 5f;
    //Can we rotate camera, which means we are not blocking the view
    public bool canRotate = true;
    private Vector2 swipeDirection; //swipe delta vector2
    private Vector2 touch1OldPos;
    private Vector2 touch2OldPos;
    private Vector2 touch1CurrentPos;
    private Vector2 touch2CurrentPos;
    private Quaternion currentRot; // store the quaternion after the slerp operation
    private Quaternion targetRot;
    private Touch touch;
    //Mouse rotation related
    private float rotX; // around x
    private float rotY; // around y
    //Mouse Scroll
    private float targetFOV;
    private float cameraFOVDamp; //Damped value
    private float fingersDistance;
    private float fovChangeVelocity = 0;
    private float distanceBetweenCameraAndTarget;


    //Scroll with Buttons
    private float h_scroll;
    private float v_scroll;
    private float moveOrizontal;
    public float moveVertical;
    public float mouseScrollMultiper = 0.1F;
    public float mobileScrollMultiper = 0.15F;

    //Clamp Value
    public float minXRotAngle_editor = -80; //min angle around x axis
    public float maxXRotAngle_editor = 5; // max angle around x axis
    public float minXRotAngle_mobile = -5; //min angle around x axis
    public float maxXRotAngle_mobile = 80; // max angle around x axis
    public float minCameraFieldOfView = 10;
    public float maxCameraFieldOfView = 70;
    public float clampHtranslate = 10;
    public float clampVtranslateDWN = 2;
    public float clampVtranslateUP = 2;
    public float clampZtranslate = 2;


 
    Vector3 dir;
    private void Awake()
    {

        GetCameraReference();
    }


    void Start()
    {
        distanceBetweenCameraAndTarget = Vector3.Distance(sceneCamera.transform.position, target.position);
        dir = new Vector3(0, 0, distanceBetweenCameraAndTarget);//assign value to the distance between the maincamera and the target
        sceneCamera.transform.position = target.position + dir; //Initialize camera position
        cameraFOVDamp = sceneCamera.fieldOfView;
        targetFOV = sceneCamera.fieldOfView;

        if (AudioManager.audioListener != null)
            Destroy(GetComponent<AudioListener>());

        DefautlView();
        StartCoroutine(StartView());
    }

    IEnumerator StartView()
    {
        yield return new WaitForSeconds(2F);
        DefautlView();
    }

    void Update()
    {

        if (!canRotate)
        {
            return;
        }
        //We are in editor
        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            EditorCameraInput();
        }
        else //We are in mobile mode
        {
            TouchCameraInput();
        }


        if (Input.GetKeyDown(KeyCode.F))
        {
            DefautlView();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            TopView();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LeftView();
        }


    }
    private void LateUpdate()
    {
        if (!canRotate)
        {
            return;
        }
        TraslateCamera();
        RotateCamera();
        SetCameraFOV();



        //if (cameraParent.position.y < minCameraPosition)
        //    cameraParent.position = new Vector3(cameraParent.position.x, minCameraPosition, cameraParent.position.z);
    }
    public void GetCameraReference()
    {
        if (sceneCamera == null)
        {
            if (InteractionManager.sceneCam)
                sceneCamera = InteractionManager.sceneCam;
            else
                sceneCamera = Camera.main;
        }
        cameraParent = transform.parent;


    }
    //May be the problem with Euler angles
    public void TopView()
    {
        rotX = -80;
        rotY = 0;
        swipeDirection.y = maxXRotAngle_mobile;
        swipeDirection.x = 0;
        targetFOV = 70;
    }
    public void LeftView()
    {
        rotY = 90;
        rotX = 0;
    }
    public void DefautlView()
    {
        rotX = startRotation.x;
        rotY = startRotation.y;
    }
    private void EditorCameraInput()
    {
        //Camera Rotation
        if (Input.GetMouseButton(0))
        {
            rotX += Input.GetAxis("Mouse Y") * mouseRotateSpeed; // around X
            rotY += Input.GetAxis("Mouse X") * mouseRotateSpeed;

            if (rotX < minXRotAngle_editor)
            {
                rotX = minXRotAngle_editor;
            }
            else if (rotX > maxXRotAngle_editor)
            {
                rotX = maxXRotAngle_editor;
            }
        }
        //Camera Field Of View
        if (Input.mouseScrollDelta.magnitude > 0)
        {
            targetFOV += Input.mouseScrollDelta.y * editorFOVSensitivity * -1;//-1 make FOV change natual
        }
    }


    private void TouchCameraInput()
    {
        if (Input.touchCount > 0)
        {
            if (Input.touchCount == 1)
            {
                touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                   
                    Debug.Log("Touch Began");
                }
                else if (touch.phase == TouchPhase.Moved)  // the problem lies in we are still rotating object even if we move our finger toward another direction
                {
                    swipeDirection += -touch.deltaPosition * touchRotateSpeed; //-1 make rotate direction natural

                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    InteractionManager.instance.SceneObjectsInteractions(true);
                   // targetFOV= cameraFOVDamp;
                }

            }
            else if (Input.touchCount == 2)
            {
                InteractionManager.instance.SceneObjectsInteractions(false);
               
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);
                if (touch1.phase == TouchPhase.Began && touch2.phase == TouchPhase.Began)
                {
                    touch1OldPos = touch1.position;
                    touch2OldPos = touch2.position;
                }
                if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
                {
                    touch1CurrentPos = touch1.position;
                    touch2CurrentPos = touch2.position;
                    fingersDistance = Vector2.Distance(touch1CurrentPos, touch2CurrentPos) - Vector2.Distance(touch1OldPos, touch2OldPos);
                    targetFOV += fingersDistance * -1 * touchFOVSensitivity; // Make rotate direction natual
                    touch1OldPos = touch1CurrentPos;
                    touch2OldPos = touch2CurrentPos;
                }

               
            }

        }

        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            if (swipeDirection.y < minXRotAngle_editor)
            {
                swipeDirection.y = minXRotAngle_editor;
            }
            else if (swipeDirection.y > maxXRotAngle_editor)
            {
                swipeDirection.y = maxXRotAngle_editor;
            }
        }
        else
        {
            if (swipeDirection.y < minXRotAngle_mobile)
            {
                swipeDirection.y = minXRotAngle_mobile;
            }
            else if (swipeDirection.y > maxXRotAngle_mobile)
            {
                swipeDirection.y = maxXRotAngle_mobile;
            }

        }
        if (touch.phase == TouchPhase.Ended)
        {
            InteractionManager.instance.SceneObjectsInteractions(true);
        }
    }
    private void RotateCamera()
    {

        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            Vector3 tempV = new Vector3(rotX, rotY, 0);
            targetRot = Quaternion.Euler(tempV); //We are setting the rotation around X, Y, Z axis respectively
        }
        else
        {
            targetRot = Quaternion.Euler(-swipeDirection.y, swipeDirection.x, 0);
        }
        //Rotate Camera
        currentRot = Quaternion.Slerp(currentRot, targetRot, Time.smoothDeltaTime * rotationSmoothValue * 50);  //let cameraRot value gradually reach newQ which corresponds to our touch
                                                                                                             //Multiplying a quaternion by a Vector3 is essentially to apply the rotation to the Vector3
                                                                                                             //This case it's like rotate a stick the length of the distance between the camera and the target and then look at the target to rotate the camera.

        Vector3 addTranslateMovements = (sceneCamera.transform.right * moveOrizontal + sceneCamera.transform.up * moveVertical);

        Vector3 lookAt = cameraParent.position+target.position + addTranslateMovements;
        sceneCamera.transform.position = cameraParent.position+(target.position + currentRot * dir) ;
        cameraParent.position += addTranslateMovements;

        float clampedX = cameraParent.position.x;
        float clampedY = cameraParent.position.y;
        float clampedZ = cameraParent.position.z;

        clampedX = Mathf.Clamp(clampedX, -clampHtranslate, clampHtranslate);
        clampedY = Mathf.Clamp(clampedY, -clampVtranslateDWN, clampVtranslateUP);
        clampedZ = Mathf.Clamp(clampedZ, -clampZtranslate , clampZtranslate);

        cameraParent.position = new Vector3(clampedX, clampedY, clampedZ);
        sceneCamera.transform.LookAt(lookAt);


    }
    void SetCameraFOV()
    {
        //Set Camera Field Of View
        //Clamp Camera FOV value
        //if (cameraFieldOfView <= minCameraFieldOfView)
        //{
        //    cameraFieldOfView = minCameraFieldOfView;
        //}
        //else if (cameraFieldOfView >= maxCameraFieldOfView)
        //{
        //    cameraFieldOfView = maxCameraFieldOfView;
        //}


        // cameraFOVDamp = Mathf.SmoothDamp(cameraFOVDamp, cameraFieldOfView, ref fovChangeVelocity, zoomSmoothTime);

        cameraFOVDamp = Mathf.Lerp(cameraFOVDamp, targetFOV, zoomSmoothTime * Time.deltaTime*10);
        targetFOV = Mathf.Clamp(targetFOV, minCameraFieldOfView, maxCameraFieldOfView);
        cameraFOVDamp = Mathf.Clamp(cameraFOVDamp, minCameraFieldOfView, maxCameraFieldOfView);

        sceneCamera.fieldOfView = cameraFOVDamp;
    }

    private void TraslateCamera()
    {
        if (!CameraCollision.collision)
        {
            moveOrizontal += h_scroll * Time.deltaTime * 30;
            moveVertical += v_scroll * Time.deltaTime * 30;
        }


        moveOrizontal = Mathf.Clamp(moveOrizontal, -2, 2);
        moveVertical = Mathf.Clamp(moveVertical, -2, 2);

        //Rallenta
        moveVertical = Mathf.Lerp(moveVertical, 0, Time.deltaTime * 2);
        moveOrizontal = Mathf.Lerp(moveOrizontal, 0, Time.deltaTime * 2);

    }



    public void HorizontalScroll(float direction)
    {
        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            h_scroll = direction* mouseScrollMultiper;
        else
            h_scroll = direction * mobileScrollMultiper;
    }

    public void VerticalScroll(float direction)
    {
        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            v_scroll = direction* mouseScrollMultiper;
        else
            v_scroll = direction * mobileScrollMultiper;

    }
    public void EndVscroll()
    {
        v_scroll = 0;

    }
    public void EndHscroll()
    {
        h_scroll = 0;


    }


}