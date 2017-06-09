using UnityEngine;

[AddComponentMenu("Camera Controllers/Better Mouse Orbit")]
public class CameraController : MonoBehaviour
{
    public enum MouseButton
    {
        LeftOrRightMouseButton = -2,
        None = -1,
        LeftMouseButton = 0,
        RightMouseButton = 1,
        MiddleMouseButton = 2
    };

    public Transform target;

    public float distance = 10f;                    // The user's desired zoom distance (changes on scrollwheel)
    private float _distance;                        // The true camera distance
    public float distanceMin = 0f;                  // Closest distance the camera can be from target
    public float distanceMax = 15f;                 // Furthest distance the camera can be from the target

    public Vector2 orientation = Vector2.zero;      // The user's desired camera orientation (changes based on mouse movement)
    private Vector2 _orientation;                   // The true camera orientation
    public float orientationYMin = -20f;            // Lowest rotation.y angle
    public float orientationYMax = 88f;             // Highest rotation.y angle

    public Vector2 panSpeed = Vector2.one * 200f;   // Mouse x/y movement multiplier
    public float panLerpSpeed = 50f;                // Lerp speed of the camera pan
    public MouseButton panMouseButton = MouseButton.LeftOrRightMouseButton; // Mouse button that must be pressed while panning.
    public bool panLocksMouse = false;              // Does the mouse lock to the game window when panning?

    public float zoomSpeed = 12f;                    // Mouse scrollwheel multiplier
    public float zoomLerpSpeed = 10f;               // Lerp speed of the camera zoom
    public float zoomLerpClipSpeed = 50f;           // Lerp speed of the camera zoom when forced to move because of a raycast hit

    public float minSurfaceDistance = 0.3f;         // Desired distance the camera should sit away from surface of a raycast hit
    public LayerMask raycastLayerMask = -1;         // Which layers are valid for a raycast hit (default: -1, Everything)

    void Start()
    {
        _distance = distance;
        _orientation = orientation;
    }

    void LateUpdate()
    {
        if (target)
        {
            // determine distance
            float prevDistance = _distance;

            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, distanceMin, distanceMax);
            _distance = Mathf.Lerp(_distance, distance, Time.deltaTime * zoomLerpSpeed);
            
            // mouse button active?
            bool mouseButtonActive;
            if (panMouseButton == MouseButton.None) {
                mouseButtonActive = true;
            } else if (panMouseButton == MouseButton.LeftOrRightMouseButton) {
                mouseButtonActive = (Input.GetMouseButton(0) || Input.GetMouseButton(1)) ? true : false;
            } else {
                mouseButtonActive = Input.GetMouseButton((int)panMouseButton);
            }

            // cursor locked?
            if (panLocksMouse) {
                Cursor.lockState = mouseButtonActive ? CursorLockMode.Confined : CursorLockMode.None;
                // cursor is visible when mouse is not active
                Cursor.visible = !mouseButtonActive;
            }

            if (mouseButtonActive)
            {
                orientation += new Vector2(
                    Input.GetAxis("Mouse X") * panSpeed.x,
                    -Input.GetAxis("Mouse Y") * panSpeed.y) * Time.deltaTime;
            }
            orientation.x = ClampAngle(orientation.x);
            orientation.y = ClampAngle(orientation.y, orientationYMin, orientationYMax);
            _orientation.x = Mathf.LerpAngle(_orientation.x, orientation.x, Time.deltaTime * panLerpSpeed);
            _orientation.y = Mathf.LerpAngle(_orientation.y, orientation.y, Time.deltaTime * panLerpSpeed);
            
            Quaternion rotation = Quaternion.Euler(_orientation.y, _orientation.x, 0);
            Vector3 ray = rotation * Vector3.back;

            // camers clipping?
            RaycastHit hit;
            if (Physics.SphereCast(target.position, minSurfaceDistance, ray, out hit, distance, raycastLayerMask))
            {
                _distance = Mathf.Lerp(prevDistance, hit.distance, Time.deltaTime * zoomLerpClipSpeed);
            }
            
            //Debug.DrawRay(target.position, ray * distance, Color.red);
            //Debug.DrawRay(target.position, ray * _distance, Color.blue);

            // set camera's new position + rotation
            transform.position = target.position + (ray * _distance);
            transform.rotation = rotation;
        } else {
            transform.position += (transform.forward * -1) * Time.deltaTime;
        }
    }

    public static float ClampAngle(float angle)
    {
        return Mathf.Repeat(angle, 360);
    }
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}



// 1. Fade character out when scrolling in to 1st person
// 2. Fade character in when scrolling away