using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkullViewerMultiPlatform : MonoBehaviour
{
    // can change all the speeds to however you'd like
    [Header("Target & Distance")]
    public Transform target;
    public float distance = 2f;
    public float minDistance = 0.5f;
    public float maxDistance = 5f;

    [Header("Rotation Settings (Mouse & Keys)")]
    public float mouseRotationSpeed = 50f;
    public float keyRotationSpeed = 30f;
    public float minYAngle = -80f;
    public float maxYAngle = 80f;

    [Header("Rotation Settings (Touch)")]
    public float touchRotationSpeed = 0.2f;

    [Header("Zoom Settings")]
    public float keyZoomSpeed = 2f;
    public float pinchZoomSpeed = 0.01f;

    [Header("Landmark UI")]
    public TextMeshProUGUI infoText;

    [Header("Click/Drag Detection (Mouse)")]
    public float clickDragThreshold = 5f;

    [Header("Tap Threshold (Touch)")]
    public float tapThreshold = 20f;

    private float currentX = 0f;
    private float currentY = 0f;
    private bool isDraggingMouse = false;
    private Vector3 mouseDownPos;

    // single finger touch compatibility 
    private Vector2 touchStartPos;
    private bool isTap = false;

    void Start()
    {
        if (infoText != null) infoText.text = "";
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        if (target == null)
        {
            Debug.LogWarning("SkullViewerMultiPlatform: No target assigned!");
        }
    }

    void Update()
    {
        if (target == null) return;

        if (Input.touchCount == 0)
        {
            // for desktop users
            HandleMouseAndKeyboard();
        }
        else
        {
            // for mobile device users 
            HandleTouchInput();
        }

        // press 'd' to clear text on desktop 
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (infoText != null) infoText.text = "";
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0f);
        Vector3 dir = new Vector3(0, 0, -distance);

        transform.position = rotation * dir + target.position;
        transform.LookAt(target.position);
    }

    
    // define mouse & keys for desktop 
    void HandleMouseAndKeyboard()
    {
        // click & rotate
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPos = Input.mousePosition;
            isDraggingMouse = false;
        }

        if (Input.GetMouseButton(0))
        {
            float dragDist = Vector3.Distance(Input.mousePosition, mouseDownPos);
            if (dragDist > clickDragThreshold)
            {
                isDraggingMouse = true;
            }

            if (isDraggingMouse)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                currentX += mouseX * mouseRotationSpeed * Time.deltaTime;
                currentY -= mouseY * mouseRotationSpeed * Time.deltaTime;
                currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!isDraggingMouse)
            {
                // single click -> check if its a landmark area to populate fact
                CheckLandmarkClick(Input.mousePosition);
            }
        }

        // use arrow keys to rotate object
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            currentX -= keyRotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            currentX += keyRotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            currentY += keyRotationSpeed * Time.deltaTime;
            currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            currentY -= keyRotationSpeed * Time.deltaTime;
            currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
        }

        // use 'i' and 'o' keys to zoom in/out of object 
        if (Input.GetKey(KeyCode.I))
        {
            distance -= keyZoomSpeed * Time.deltaTime;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
        if (Input.GetKey(KeyCode.O))
        {
            distance += keyZoomSpeed * Time.deltaTime;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // scroll with moude (or two finger on keypad) -> zoom in/out 
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            distance -= scroll * keyZoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    // mobile touch screen 
    void HandleTouchInput()
    {
        // one finger 
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                touchStartPos = t.position;
                isTap = true;
            }
            else if (t.phase == TouchPhase.Moved)
            {
                float dist = (t.position - touchStartPos).magnitude;
                if (dist > tapThreshold)
                {
                    isTap = false; // moving object around 

                    float dx = t.deltaPosition.x * touchRotationSpeed;
                    float dy = t.deltaPosition.y * touchRotationSpeed;

                    currentX += dx;
                    currentY -= dy;
                    currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
                }
            }
            else if (t.phase == TouchPhase.Ended)
            {
                // just tap -> check if its a landmark area 
                if (isTap)
                {
                    CheckLandmarkClick(t.position);
                }
            }
        }
        // two finger touch -> zoom in/out 
        else if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 t0Prev = t0.position - t0.deltaPosition;
            Vector2 t1Prev = t1.position - t1.deltaPosition;

            float prevMag = (t0Prev - t1Prev).magnitude;
            float currentMag = (t0.position - t1.position).magnitude;

            float diff = currentMag - prevMag;
            distance -= diff * pinchZoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    // landmark populaion
    void CheckLandmarkClick(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            Landmark landmark = hit.collider.GetComponent<Landmark>();
            if (landmark != null)
            {
                // get data from JSON 
                LandmarkEntry entry = LandmarkDataLoader.Instance.GetLandmark(landmark.landmarkId);
                if (entry != null)
                {
                    Debug.Log("Clicked Landmark: " + entry.name);
                    if (infoText != null)
                    {
                        infoText.text = entry.name + "\n" + entry.fact;
                    }
                }
                else
                {
                    // if no id for landmark in JSON 
                    if (infoText != null) infoText.text = "";
                }
            }
            else
            {
                // clicked on smth w no landmark -> remove text
                if (infoText != null) infoText.text = "";
            }
        }
        else
        {
            // clicked empty space -> remove text
            if (infoText != null) infoText.text = "";
        }
    }
}