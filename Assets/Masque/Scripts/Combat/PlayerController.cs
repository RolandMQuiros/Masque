using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    public enum MovementStyle {
        Run,
        Strafe,
        Aim,
        Lock
    }

    public MovementStyle Movement;

    public Camera Camera;
    public float RunSpeed = 100f;
    public float AimSpeed = 40f;

    public bool CanNudge {
        get;
        private set;
    }

    private CharacterController m_characterController;
    private CameraRelativeController m_camRelController;
    private CameraSpinner m_camSpinner;

    private bool m_isNudging = false;

    public void Awake() {
        m_characterController = GetComponent<CharacterController>();
        m_camRelController = GetComponent<CameraRelativeController>();
        m_camSpinner = Camera.GetComponent<CameraSpinner>();

        CanNudge = true;
    }

    public void Nudge(Vector3 initialVelocity, float deceleration) {
        if (CanNudge) {
            StartCoroutine(NudgeProcess(initialVelocity, deceleration));
        }
    }

    public void NudgeForward(float initialSpeed, float deceleration) {
        if (CanNudge) {
            Vector3 initialVelocity = initialSpeed * (transform.rotation * Vector3.forward);
            Nudge(initialVelocity, deceleration);
        }
    }

    public void NudgeBackward(float initialSpeed, float deceleration) {
        if (CanNudge) {
            Vector3 initialVelocity = initialSpeed * (transform.rotation * Vector3.back);
            Nudge(initialVelocity, deceleration);
        }
    }
    
    private IEnumerator NudgeProcess(Vector3 initialVelocity, float deceleration) {
        Vector3 velocity = initialVelocity;
        Vector3 decelerationVector = Vector3.Normalize(initialVelocity) * deceleration;
        float speed = velocity.magnitude;

        while (speed > 0f) {
            float dt = Time.deltaTime;

            m_isNudging = true;
            velocity -= decelerationVector * dt * dt;

            m_camRelController.Move(velocity, false);

            speed -= deceleration * dt * dt;

            yield return null;
        }
        m_isNudging = false;
    }

    public void OnTriggerEnter(Collider other) {
        Debug.Log(other.ToString());
    }

    public void Update() {
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        // Update movement
        switch (Movement) {
            case MovementStyle.Run:
                m_camRelController.Move(hAxis, vAxis, RunSpeed, true);
                break;
            case MovementStyle.Strafe:
                m_camRelController.Move(hAxis, vAxis, AimSpeed, false);
                break;
            case MovementStyle.Lock:
                m_camRelController.Move(0f, 0f, RunSpeed, false);
                // No movement
                break;
        }

        float rotateAxis = Input.GetAxis("RotateHorizontal");
        if (m_camSpinner && rotateAxis != 0f) {
            m_camSpinner.Angle += 5f * rotateAxis;
        }
    }
}
