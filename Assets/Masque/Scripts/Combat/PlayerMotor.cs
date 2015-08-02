using UnityEngine;
using System.Collections;

/// <summary>
/// Handles motion controls for Player, both mouse+keyboard and gamepad configuration.
/// </summary>
[RequireComponent(typeof(MotionBuffer))]
[RequireComponent(typeof(PlayerAim))]
public class PlayerMotor : MonoBehaviour {
    public const float DEFAULT_TURN_SMOOTHING = 10f;
    public float DIRECTION_CHANGE_THRESHOLD = 0.05f;
    public string ANIM_WALKING_SPEED = "Walking Speed";

    
    public Camera Camera;
    public float MovementSpeed = 10f;

    public PlanarViewVectors ControlPlane;

    public enum MovementStyle {
        Free, // Move freely in all directions.  Player faces direction of crosshair
        Strafe, // Move freely in all directions, but player's direction is locked
        Pivot, // Player is stationary, facing crosshair
        Lock // Player cannot move or change direction
    }
    public MovementStyle Movement;

    /// <summary>
    /// The velocity of this PlayerMotor's movement.
    /// </summary>
    public Vector3 Velocity { get; private set; }
    /// <summary>
    /// The rotation of this PlayerMotor's movement, calculated from the last non-zero value of Velocity.
    /// </summary>
    public Quaternion Rotation { get; private set; }
    /// <summary>
    /// The direction vector of this PlayerMotor's movement. Simply the normalized version of the the last non-zero
    /// value of Velocity.
    /// </summary>
    public Vector3 Direction { get; private set; }

    #region components
    private MotionBuffer m_motion;
    private Animator m_animator;
    private PlayerAim m_aim;
    #endregion

    private Quaternion m_targetRotation;
    private int m_animIsWalking;
    private int m_animWalkingSpeed;

    public Vector3 DebugVelocity;

    public void Awake() {
        m_motion = GetComponent<MotionBuffer>();
        m_animator = GetComponent<Animator>();
        m_aim = GetComponent<PlayerAim>();

        m_animWalkingSpeed = Animator.StringToHash(ANIM_WALKING_SPEED);
    }

    public void Start() {
        if (Camera == null) {
            Camera = Camera.main;
        }
        ControlPlane = new PlanarViewVectors(Camera.transform);
    }

    public void Update() {
        Vector2 axes = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

        // Update movement
        Quaternion aimRot = m_aim.Rotation;
        switch (Movement) {
            case MovementStyle.Free:
                Velocity = Move(axes, MovementSpeed);
                RotateTo(aimRot);
                break;
            case MovementStyle.Strafe:
                Velocity = Move(axes, MovementSpeed);
                break;
            case MovementStyle.Pivot:
                Velocity = Vector3.zero;
                RotateTo(aimRot);
                break;
            case MovementStyle.Lock:
                Velocity = Vector3.zero;
                break;
        }

        if (Velocity != Vector3.zero) {
            Rotation = Quaternion.LookRotation(Velocity, ControlPlane.Up);
            Direction = Velocity.normalized;
        }

        DebugVelocity = Velocity;
    }

    public void LateUpdate() {
        ControlPlane.RefreshBaseVectors(Camera.transform);
    }

    public Vector3 Move(Vector2 axes, float speed, bool turnToMotion = false, float turnSmoothing = DEFAULT_TURN_SMOOTHING) {
        Vector3 direction = ControlPlane.Transform(axes);

        Vector3 velocity = direction * speed * Time.deltaTime;
        if (m_motion.enabled) {
            m_motion.Move(velocity);
        }

        if (turnToMotion) {
            RotateTo(direction);
        }

        return velocity;
    }

    public void RotateTo(Vector3 direction, float turnSmoothing = DEFAULT_TURN_SMOOTHING) {
        if (direction != Vector3.zero) {
            m_targetRotation = Quaternion.LookRotation(direction, ControlPlane.Up);
            Quaternion newRotation = Quaternion.Lerp(transform.rotation, m_targetRotation, turnSmoothing * Time.deltaTime);
            transform.rotation = newRotation;
        }
    }

    public void RotateTo(Quaternion rotation, float turnSmoothing = DEFAULT_TURN_SMOOTHING) {
        m_targetRotation = rotation;
        Quaternion newRotation = Quaternion.Lerp(transform.rotation, m_targetRotation, turnSmoothing * Time.deltaTime);
        transform.rotation = newRotation;
    }

    public bool WarpTo(Vector3 position, float radius) {
        NavMeshHit hit;
        bool canWarp = NavMesh.SamplePosition(position, out hit, radius, NavMesh.AllAreas);

        if (canWarp) {
            m_motion.Warp(hit.position);
        }

        return canWarp;
    }
}
