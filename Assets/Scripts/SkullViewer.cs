using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkullViewerMultiPlatform : MonoBehaviour
{
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

    // For single-finger touch
    private Vector2 touchStartPos;
    private bool isTap = false;

    // Reference to the camera for raycasting
    private Camera mainCamera;

    void Start()
    {
        if (infoText != null) infoText.text = "";
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        if (target == null)
        {
            Debug.LogWarning("SkullViewerMultiPlatform: No target assigned!");
        }

        // Cache the main camera reference
        mainCamera = Camera.main;
        
        // Force initialization of the LandmarkDataLoader
        Debug.Log("LandmarkDataLoader instance exists: " + (LandmarkDataLoader.Instance != null));
    }

    void Update()
    {
        if (target == null) return;

        if (Input.touchCount == 0)
        {
            // Desktop approach
            HandleMouseAndKeyboard();
        }
        else
        {
            // Mobile/touch approach
            HandleTouchInput();
        }

        // Press D to clear text
        if (Input.GetKeyDown(KeyCode.D))
        {
            ClearInfoText();
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

    // Helper method to clear info text
    private void ClearInfoText()
    {
        if (infoText != null) infoText.text = "";
    }

    // -----------------------
    // Desktop: Mouse & Keys
    // -----------------------
    void HandleMouseAndKeyboard()
    {
        // 1) Mouse: Click & Drag
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
                // It's a click => do a landmark check
                CheckLandmarkClick(Input.mousePosition);
            }
        }

        // 2) Arrow Key Rotation
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

        // 3) I/O keys for Zoom
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

        // 4) Mouse Scroll => Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            distance -= scroll * keyZoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    // -----------------------
    // Mobile/Touch Approach
    // -----------------------
    void HandleTouchInput()
    {
        // Single finger => rotate or tap
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
                    isTap = false; // it's a drag

                    float dx = t.deltaPosition.x * touchRotationSpeed;
                    float dy = t.deltaPosition.y * touchRotationSpeed;

                    currentX += dx;
                    currentY -= dy;
                    currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
                }
            }
            else if (t.phase == TouchPhase.Ended)
            {
                // If it's still a tap, do the landmark check
                if (isTap)
                {
                    CheckLandmarkClick(t.position);
                }
            }
        }
        // Two-finger => pinch zoom
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

    // -------------
    // Landmark Raycast
    // -------------
    void CheckLandmarkClick(Vector2 screenPos)
    {
        // Use cached camera reference
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        RaycastHit hit;
        
        Debug.Log("Casting ray from screen position: " + screenPos);
        
        if (Physics.Raycast(ray, out hit, 100f))
        {
            Debug.Log("Hit something: " + hit.collider.name);
            
            Landmark landmark = hit.collider.GetComponent<Landmark>();
            if (landmark != null)
            {
                Debug.Log("Found Landmark component with ID: " + landmark.landmarkId);
                
                // Look up data from our non-MonoBehaviour LandmarkDataLoader
                LandmarkEntry entry = LandmarkDataLoader.Instance.GetLandmark(landmark.landmarkId);
                if (entry != null)
                {
                    Debug.Log("Clicked Landmark: " + entry.name + " with fact: " + entry.fact);
                    if (infoText != null)
                    {
                        infoText.text = entry.name + "\n" + entry.fact;
                        Debug.Log("Set infoText to: " + infoText.text);
                    }
                    else
                    {
                        Debug.LogError("infoText UI component is null!");
                    }
                }
                else
                {
                    Debug.LogWarning("No entry found for landmarkId: " + landmark.landmarkId);
                    // If no matching ID in dictionary
                    ClearInfoText();
                }
            }
            else
            {
                Debug.Log("Hit object has no Landmark component");
                // If we clicked something with no Landmark, clear text
                ClearInfoText();
            }
        }
        else
        {
            Debug.Log("Ray did not hit anything");
            // If we clicked/tapped empty space, clear text
            ClearInfoText();
        }
    }
}
