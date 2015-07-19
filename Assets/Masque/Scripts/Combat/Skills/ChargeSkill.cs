using UnityEngine;
using System.Collections;

public class ChargeSkill : PlayerSkill {
    public float DistanceThreshold = 0.5f;
    public float MaxChargeSpeed = 40f;
    public float Acceleration = 15f;
    public float Friction = 25f;
    public float TurnSpeed = 1f;
    public float CrashSpeedThreshold = 25f;
    public float GlancingAngleThreshold = 30f;
    public float RecoilElasticity = 0.5f;

    private PlayerAim m_aim;
    private PlayerMotor m_motor;
    private MotionBuffer m_motion;
    private Launchable m_launchable;

    private bool m_isCharging;
    private float m_speed;
    private Quaternion m_endRotation;
	
	public void Awake () {
        m_aim = GetComponent<PlayerAim>();
        m_motor = GetComponent<PlayerMotor>();
        m_motion = GetComponent<MotionBuffer>();
        m_launchable = GetComponent<Launchable>();
	}

    public override void Press() {
        if (!m_isCharging) {
            NavMeshHit hit;
            Vector3 testOffset = transform.rotation * Vector3.forward * DistanceThreshold;
            if (!m_motion.Agent.Raycast(transform.position + testOffset, out hit)) {
                m_motor.enabled = false;
                m_isCharging = true;
            }
        }
    }

    public override void Hold() {
        if (m_isCharging) {
            Quaternion newRotation = Quaternion.Lerp(transform.rotation, m_aim.Rotation, TurnSpeed * Time.deltaTime);
            transform.rotation = newRotation;
            m_endRotation = transform.rotation;

            float newSpeed = m_speed + (Acceleration * Time.deltaTime);
            m_speed = Mathf.Clamp(newSpeed, 0f, MaxChargeSpeed);
        }
    }

    public override void Release() {
        if (m_isCharging) {
            m_motor.enabled = true;
            m_isCharging = false;
        }
    }

    public void Update() {
        if (m_speed > 0f) {
            Vector3 offset = m_endRotation * (m_speed * Time.deltaTime * Vector3.forward);
            NavMeshHit hit;
            bool hasHit = m_motion.Agent.Raycast(transform.position + offset, out hit);

            if (!hasHit || Vector3.Angle(-offset, hit.normal) >= GlancingAngleThreshold) {
                if (!m_isCharging) {
                    float newSpeed = m_speed - (Friction * Time.deltaTime * Time.deltaTime);
                    m_speed = Mathf.Clamp(newSpeed, 0f, MaxChargeSpeed);
                }

                m_motion.Move(offset);
            } else if (m_speed > CrashSpeedThreshold) {
                // EXPLOOOSION
                Vector3 reflection = Vector3.Reflect(offset.normalized, hit.normal);
                m_launchable.Launch(reflection * m_speed * RecoilElasticity, Friction);

                m_motor.enabled = true;
                m_isCharging = false;
                m_speed = 0f;
            } else {
                m_motor.enabled = true;
                m_isCharging = false;
                m_speed = 0f;
            }
        }
    }
}
