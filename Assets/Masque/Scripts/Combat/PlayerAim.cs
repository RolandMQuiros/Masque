using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerAim : MonoBehaviour {
    public enum InputDevice {
        Mouse,
        Gamepad
    }
    public InputDevice InputMode;

    public Camera Camera;
    public float CursorSpeed = 100f;
    public float InnerRadius = 5f;
    public float OuterRadius = 20f;
    public bool IsHidden {
        get;
        private set;
    }

    public GameObject CursorDisplay;
    public Vector3 Target;

    private Vector3 m_cursorPos;
    private PlayerMotor m_movement;
    private PlanarViewVectors m_plane;

    public void Awake() {
        m_movement = GetComponent<PlayerMotor>();
        m_plane = GetComponent<PlanarViewVectors>();
    }

    public void Start() {
        if (Camera == null) {
            Camera = Camera.main;
        }

        if (InputMode == InputDevice.Gamepad) {
            CursorDisplay.SetActive(false);
        }
    }

    public void Show() {
        if (IsHidden && InputMode == InputDevice.Gamepad && !CursorDisplay.activeSelf) {
            CursorDisplay.SetActive(true);
            m_cursorPos = Camera.WorldToScreenPoint(transform.position);
        }
        IsHidden = false;
    }

    public void Hide() {
        if (!IsHidden && InputMode == InputDevice.Gamepad) {
            CursorDisplay.SetActive(false);
        }
        IsHidden = true;
    }

    public void Update() {
        switch (InputMode) {
            case InputDevice.Mouse:
                m_cursorPos = Input.mousePosition;
                break;
            case InputDevice.Gamepad:
                Vector3 axes = new Vector3(Input.GetAxis("Horizontal"),
                                           Input.GetAxis("Vertical"),
                                           0f);

                Vector3 offset = axes * CursorSpeed * Time.deltaTime;
                m_cursorPos += offset;
                break;
        }

        m_cursorPos.x = Mathf.Clamp(m_cursorPos.x, 0f, Screen.width);
        m_cursorPos.y = Mathf.Clamp(m_cursorPos.y, 0f, Screen.height);

        CursorDisplay.transform.position = m_cursorPos;
        Target = ScreenToWorldPoint(m_cursorPos);
    }

    private Vector3 ScreenToWorldPoint(Vector3 screenPoint) {
        Ray cursorRay = Camera.ScreenPointToRay(screenPoint); // Emit a ray from screen
        float rayDist;
        m_plane.Plane.Raycast(cursorRay, out rayDist); // Find where on the ray it intersects with the player's current
                                                       // control plane

        return cursorRay.GetPoint(rayDist); // That intersection point is the cursor's world position
    }
}
