using UnityEngine;
using System.Collections;

/// <summary>
/// Handles motion controls for Player, both mouse+keyboard and gamepad configuration.
/// </summary>
[RequireComponent(typeof(PlanarViewVectors))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PlayerAim))]
public class PlayerMotor : MonoBehaviour {
    public const float DEFAULT_TURN_SMOOTHING = 10f;
    public float DIRECTION_CHANGE_THRESHOLD = 0.05f;
    public string ANIM_WALKING_SPEED = "Walking Speed";

    public Camera Camera;
    public float RunSpeed = 100f;
    public float AimSpeed = 40f;

    public enum MovementStyle {
        Free, // Move freely in all directions.  Player faces direction of motion
        Strafe, // Move freely in all directions, but player's direction is locked
        Pivot, // Player is stationary, but changes direction
        Aim, // Player is stationary and rotates towards the now-enabled crosshair
        Lock // Player cannot move or change direction
    }
    public MovementStyle Movement;

    public Vector3 CurrentVelocity {
        get;
        private set;
    }

    #region components
    private PlanarViewVectors m_viewPlane;
    private NavMeshAgent m_agent;
    private CameraSpinner m_camSpinner;
    private Animator m_animator;
    private PlayerAim m_aim;
    #endregion

    private Vector3 m_targetDirection;
    private int m_animIsWalking;
    private int m_animWalkingSpeed;

    public float DebugVelocity;

    public void Awake() {
        m_agent = GetComponent<NavMeshAgent>();
        m_viewPlane = GetComponent<PlanarViewVectors>();
        m_camSpinner = Camera.GetComponent<CameraSpinner>();
        m_animator = GetComponent<Animator>();
        m_aim = GetComponent<PlayerAim>();

        m_animWalkingSpeed = Animator.StringToHash(ANIM_WALKING_SPEED);
    }

    public void Start() {
        m_viewPlane.RefreshBaseVectors(Camera.transform);
    }

    public void Update() {
        Vector2 axes = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

        // Update movement
        switch (Movement) {
            case MovementStyle.Free:
                CurrentVelocity = Move(axes, RunSpeed, true);
                break;
            case MovementStyle.Strafe:
                CurrentVelocity = Move(axes, AimSpeed, false);
                break;
            case MovementStyle.Pivot:
                CurrentVelocity = Move(axes, 0f, true);
                break;
            case MovementStyle.Aim:
                CurrentVelocity = Vector3.zero;
                Vector3 toTarget = (m_aim.Target - transform.position);
                toTarget.y = 0f;
                RotateTo(toTarget.normalized);
                break;
        }

        float rotateAxis = Input.GetAxis("RotateHorizontal");
        if (m_camSpinner && rotateAxis != 0f) {
            m_camSpinner.Angle += 5f * rotateAxis;
        }
    }

    public Vector3 Move(Vector2 axes, float speed, bool turning = true, float turnSmoothing = DEFAULT_TURN_SMOOTHING) {
        m_viewPlane.RefreshBaseVectors(Camera.transform);
        Vector3 direction = m_viewPlane.Apply(axes);

        Vector3 velocity = direction * speed * Time.deltaTime;
        if (m_agent.enabled) {
            m_agent.Move(velocity);
        }

        if (turning) {
            RotateTo(direction);
        }
        return velocity;
    }

    public void RotateTo(Vector3 direction, float turnSmoothing = DEFAULT_TURN_SMOOTHING) {
        if (direction != Vector3.zero) {
            m_targetDirection = direction;
        }

        if (m_targetDirection != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(m_targetDirection, Vector3.up);
            Quaternion newRotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSmoothing * Time.deltaTime);

            transform.rotation = newRotation;
        }
    }


    public bool WarpTo(Vector3 position, float radius) {
        NavMeshHit hit;
        bool canWarp = NavMesh.SamplePosition(position, out hit, radius, NavMesh.AllAreas);

        if (canWarp) {
            m_agent.Warp(hit.position);
        }

        return canWarp;
    }

    private void UpdateCrossHair() {

    }
}
