using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerAim : MonoBehaviour {
    public const float MOUSE_ATTENUATION = 0.01f;

    public enum InputDevice {
        Mouse,
        Gamepad
    }
    public InputDevice InputMode;

    public Camera Camera;
    public float Sensitivity = 1f;
    public float InnerRadius = 1f;
    public float OuterRadius = 8f;
    public float DriftSpeed = 2f;

    public Vector2 DebugRawAxes;

    public bool IsHidden {
        get;
        private set;
    }

    public GameObject ScreenCursorDisplay;
    public GameObject WorldCursorDisplay;
    public Vector3 Target;

    public Quaternion Rotation { get { return m_rotation; } }
    public Vector3 Centroid { get { return ((Target - transform.position) / 2f) + transform.position; } }

    public Vector3 Direction {
        get {
            return (Target - transform.position).normalized;
        }
    }

    private PlayerMotor m_movement;
    private PlanarViewVectors m_plane;

    private Quaternion m_rotation;
    private Vector3 m_centroid;
    private Vector2 m_cursorPos;
    private Vector2 m_axes;
    private Vector3 m_offset;

    public void Awake() {
        m_movement = GetComponent<PlayerMotor>();
        m_plane = GetComponent<PlanarViewVectors>();

        Target = transform.position + (transform.rotation * Vector3.forward);

        if (InputMode == InputDevice.Mouse) {
            m_cursorPos = Input.mousePosition;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    public void Start() {
        if (Camera == null) {
            Camera = Camera.main;
        }
    }

    public void Recenter() {
        m_offset = transform.rotation * Vector3.forward;
    }

    public void Update() {
        float smallScreenHalfSize = Mathf.Min(Screen.width, Screen.height) / 2f;
        float offsetMag = 0f;

        switch (InputMode) {
            case InputDevice.Mouse:
                DebugRawAxes.Set(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
                m_axes.Set(Input.GetAxisRaw("Mouse X") * Sensitivity * Time.deltaTime,
                           Input.GetAxisRaw("Mouse Y") * Sensitivity * Time.deltaTime);

                m_offset += ((m_plane.Forward * m_axes.y) + (m_plane.Right * m_axes.x));
                offsetMag = m_offset.magnitude;
                break;
            case InputDevice.Gamepad:
                DebugRawAxes.Set(Input.GetAxisRaw("Aim X"), Input.GetAxisRaw("Aim Y"));
                m_axes.Set(Input.GetAxisRaw("Aim X") * Sensitivity * Time.deltaTime,
                           Input.GetAxisRaw("Aim Y") * Sensitivity * Time.deltaTime);

                m_offset += ((m_plane.Forward * m_axes.y) + (m_plane.Right * m_axes.x));
                offsetMag = m_offset.magnitude;

                if (m_axes == Vector2.zero) {
                    float drift = DriftSpeed * Time.deltaTime;
                    if (offsetMag >= InnerRadius - drift) {
                        m_offset -= m_offset.normalized * DriftSpeed * Time.deltaTime;
                    } else if (offsetMag >= InnerRadius + drift) {
                        m_offset += m_offset.normalized * DriftSpeed * Time.deltaTime;
                    }
                }
                break;
        }

        if (offsetMag > OuterRadius) {
            m_offset = m_offset.normalized * OuterRadius;
        }
        Target = transform.position + m_offset;

        if (ScreenCursorDisplay != null) {
            ScreenCursorDisplay.transform.position = m_cursorPos;
        }

        if (WorldCursorDisplay != null) {
            WorldCursorDisplay.transform.position = Target;
        }
        
        Vector3 dp = Target - transform.position;
        if (dp != Vector3.zero) {
            m_rotation = Quaternion.LookRotation(dp, m_plane.Up);
            m_centroid = (dp / 2f) + transform.position;
        }
    }

    public void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(m_centroid, 1);
    }

    private Vector3 ScreenToWorldPoint(Vector2 screenPoint) {
        // Emit a ray from screen
        Ray cursorRay = Camera.ScreenPointToRay(new Vector3(screenPoint.x, screenPoint.y, 0f));

        Vector3 point = new Vector3();
        float rayDist;

        // Find where on the ray it intersects with the player's current control plane
        if (m_plane.Plane.Raycast(cursorRay, out rayDist)) {
            point = cursorRay.GetPoint(rayDist); // That intersection point is the cursor's world position
        }
        return point;
    }
}
