using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    private Vector2 swipeDirection; // kept for compatibility but not used for rotation now
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
    private float distanceBetweenCameraAndTarget;


    //Scroll with Buttons
    public float h_scroll;
    public float v_scroll;
    public float moveOrizontal;
    public float moveVertical;
    public float mouseScrollMultiper = 0.1F;
    public float mobileScrollMultiper = 0.15F;

    //Clamp Value
    public float minXRotAngle = -80; //min angle around x axis
    public float maxXRotAngle = 5; // max angle around x axis

    public float minCameraFieldOfView = 10;
    public float maxCameraFieldOfView = 70;
    public float clampHtranslate = 10;
    public float clampVtranslateDWN = 2;
    public float clampVtranslateUP = 2;
    public float clampZtranslate = 2;

    Vector3 dir;

    // track if the active single touch started over UI (ignore rotation while that touch is active)
    private bool touchStartedOverUI = false;

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

        // initialize quaternions to avoid jumps / NaN on first Slerp
        currentRot = Quaternion.Euler(rotX, rotY, 0);
        targetRot = currentRot;

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
            return;

        //We are in editor
        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            EditorCameraInputRotation();
        else //We are in mobile mode
            TouchCameraInputRotation();
    }

    private void LateUpdate()
    {
        TraslateCamera();
        SetCameraFOV();
        RotateAndMoveCamera();

        if (v_scroll + h_scroll == 0)
            StopTranslation();
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
        swipeDirection.y = maxXRotAngle;
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

    private void EditorCameraInputRotation()
    {
        // ignore mouse drag if pointer is over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        //Camera Rotation
        if (Input.GetMouseButton(0))
        {
            rotX += Input.GetAxis("Mouse Y") * mouseRotateSpeed; // around X
            rotY += Input.GetAxis("Mouse X") * mouseRotateSpeed;

            rotX = Mathf.Clamp(rotX, minXRotAngle, maxXRotAngle);
        }
        //Camera Field Of View
        if (Input.mouseScrollDelta.magnitude > 0)
        {
            targetFOV += Input.mouseScrollDelta.y * editorFOVSensitivity * -1;//-1 make FOV change natual
        }
    }

    private void TouchCameraInputRotation() //Solo per la rotazione, lo zoom e il translate sono gestito da un altro metodo per evitare conflitti tra i due tipi di input
    {
        if (Input.touchCount <= 0)
            return;

        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                // decide once if this touch started over UI
                touchStartedOverUI = (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId));
                // reset small accumulators to avoid drift
                swipeDirection = Vector2.zero;
            }
            else if (t.phase == TouchPhase.Moved)
            {
                if (!touchStartedOverUI)
                {
                    // convert pixel delta to sensible angles (scale tweak)
                    float scale = 0.02f;
                    rotX += -t.deltaPosition.y * touchRotateSpeed * scale;
                    rotY += t.deltaPosition.x * touchRotateSpeed * scale;

                    rotX = Mathf.Clamp(rotX, minXRotAngle, maxXRotAngle);
                }
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                touchStartedOverUI = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            // if either touch began over UI, ignore rotation and pinch processing
            bool t1UI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch1.fingerId);
            bool t2UI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch2.fingerId);
            if (t1UI || t2UI)
                return;

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

    private void RotateAndMoveCamera()
    {
        // use rotX/rotY as single source of truth
        targetRot = Quaternion.Euler(rotX, rotY, 0);

        //Guard: ensure quaternions valid
        if (float.IsNaN(targetRot.x) || float.IsNaN(targetRot.y) || float.IsNaN(targetRot.z) || float.IsNaN(targetRot.w))
            targetRot = Quaternion.identity;
        if (float.IsNaN(currentRot.x) || float.IsNaN(currentRot.y) || float.IsNaN(currentRot.z) || float.IsNaN(currentRot.w))
            currentRot = targetRot;

        //Rotate Camera
        currentRot = Quaternion.Slerp(currentRot, targetRot, Time.smoothDeltaTime * rotationSmoothValue * 50);

        //Move Camera
        Vector3 addTranslateMovements = (sceneCamera.transform.right * moveOrizontal + sceneCamera.transform.up * moveVertical);
        Vector3 lookAt = cameraParent.position + target.position + addTranslateMovements;
        sceneCamera.transform.position = cameraParent.position + (target.position + currentRot * dir);
        cameraParent.position += addTranslateMovements;

        float clampedX = cameraParent.position.x;
        float clampedY = cameraParent.position.y;
        float clampedZ = cameraParent.position.z;

        clampedX = Mathf.Clamp(clampedX, -clampHtranslate, clampHtranslate);
        clampedY = Mathf.Clamp(clampedY, -clampVtranslateDWN, clampVtranslateUP);
        clampedZ = Mathf.Clamp(clampedZ, -clampZtranslate, clampZtranslate);

        cameraParent.position = new Vector3(clampedX, clampedY, clampedZ);
        sceneCamera.transform.LookAt(lookAt);
    }

    void SetCameraFOV()
    {
        cameraFOVDamp = Mathf.Lerp(cameraFOVDamp, targetFOV, zoomSmoothTime * Time.deltaTime * 10);
        targetFOV = Mathf.Clamp(targetFOV, minCameraFieldOfView, maxCameraFieldOfView);
        cameraFOVDamp = Mathf.Clamp(cameraFOVDamp, minCameraFieldOfView, maxCameraFieldOfView);
        sceneCamera.fieldOfView = cameraFOVDamp;
    }

    private void TraslateCamera()
    {
        if (!CameraCollision.collision)
        {
            moveOrizontal += h_scroll * Time.deltaTime * 20;
            moveVertical += v_scroll * Time.deltaTime * 20;
        }

        moveOrizontal = Mathf.Clamp(moveOrizontal, -.15F, .15F);
        moveVertical = Mathf.Clamp(moveVertical, -.15F, .15F);
    }

    private void StopTranslation()
    {
        //Rallenta
        moveVertical = Mathf.Lerp(moveVertical, 0, Time.deltaTime * 2);
        moveOrizontal = Mathf.Lerp(moveOrizontal, 0, Time.deltaTime * 2);
    }

    public void HorizontalScroll(float direction)
    {
        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            h_scroll = direction * mouseScrollMultiper;
        else
            h_scroll = direction * mobileScrollMultiper;
    }

    public void VerticalScroll(float direction)
    {
        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            v_scroll = direction * mouseScrollMultiper;
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