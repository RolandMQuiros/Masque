using UnityEngine;
using System.Collections;


/// <summary>
/// Provides functions that allow an object to move relative to a Camera
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class CameraRelativeController : MonoBehaviour {
    public const float DEFAULT_TURN_SMOOTHING = 10f;
    private const float DIRECTION_CHANGE_THRESHOLD = 0.1f;
    private const float REFRESH_CAMERA_VECTORS_THRESHOLD = 0.1f;
    private const string ANIM_WALKING_SPEED = "Walking Speed";

    [Tooltip("Reference to camera this controller bases its directions on")]
    public Camera Camera;
    [Tooltip("Acceleration in the Y-direction, in units per second per second")]
    public float Gravity = 9.81f;

    public bool DebugIsGrounded;
    public float DebugGravityOffset;

    private Vector3 m_forward;
    private Vector3 m_right;

    private CharacterController m_controller;
    private Animator m_animator;
    private PlatformDetector m_platformDetector;
    private NavMeshAgent m_navMeshAgent;

    private int m_animIsWalking;
    private int m_animWalkingSpeed;
	
    private float m_gravityOffset = 0f;
    private Vector3 m_targetDirection;

    public void Awake () {
        m_controller = GetComponent<CharacterController>();
        m_animator = GetComponent<Animator>();
        m_platformDetector = GetComponent<PlatformDetector>();
        
        m_animWalkingSpeed = Animator.StringToHash(ANIM_WALKING_SPEED);

        m_navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void Start() {
        m_targetDirection = Vector3.forward;
    }

    private void RefreshCameraVectors() {
        if (Camera == null) {
            m_forward = Vector3.forward;
            m_right = Vector3.right;
        } else {
            m_forward = (new Vector3(Camera.transform.forward.x, 0f, Camera.transform.forward.z)).normalized;
            m_right = (new Vector3(Camera.transform.right.x, 0f, Camera.transform.right.z)).normalized;
        }
    }

    public void Move(float horizontalAxis, float verticalAxis, float speed, bool turnTowardsMotion = true, float turnSmoothing = DEFAULT_TURN_SMOOTHING) {
        RefreshCameraVectors();

        // Calculate the speed in the XZ plane
        Vector3 velocity = ((m_forward * verticalAxis) + (m_right * horizontalAxis)) * speed;
        
        Move(velocity, turnTowardsMotion, turnSmoothing);
    }

    public void Move(Vector3 velocity, bool turnTowardsMotion = true, float turnSmoothing = DEFAULT_TURN_SMOOTHING) {
        RefreshCameraVectors();

        // Account for gravity
        Vector3 offset = velocity * Time.deltaTime;

        // Note: CharacterController.isGrounded is shit, so we're just doing a linecast
        if (m_platformDetector.WasPlatformFound) {
            m_gravityOffset = 0f;
            offset += m_platformDetector.Offset;
        } else {
            m_gravityOffset -= Gravity * Time.deltaTime;
            offset.y += m_gravityOffset;
        }

        
        //if (m_controller.enabled) {
        //    m_controller.Move(offset);
        //}
        m_navMeshAgent.Move(offset);

        float velocityMag = velocity.magnitude;

        // Handle rotation
        if (turnTowardsMotion) {
            if (velocityMag > DIRECTION_CHANGE_THRESHOLD) {
                m_targetDirection = velocity.normalized;
            }

            Quaternion targetRotation = Quaternion.LookRotation(m_targetDirection, Vector3.up);
            Quaternion newRotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSmoothing * Time.deltaTime);

            transform.rotation = newRotation;
        }

        // Set animation state parameters.  
        // TODO: Move this out of this MonoBehaviour so it can be handled by the PlayerController.
        m_animator.SetFloat(m_animWalkingSpeed, velocityMag);

        DebugIsGrounded = m_platformDetector.WasPlatformFound;
        DebugGravityOffset = m_gravityOffset;
    }
}
