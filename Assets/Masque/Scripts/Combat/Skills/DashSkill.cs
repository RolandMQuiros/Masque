using UnityEngine;
using System.Collections;

public class DashSkill : PlayerSkill {
    public float LongDashChargeThreshold = 0.1f;

    public float ShortDashDistance = 10f;
    public float ShortDashTime = 0.6f;

    public float FreezeMovmentControlTime = 0.2f;
    public float InvincibilityTime = 0.5f;

    public float Radius = 20f;

    private float m_chargeTime = 0f;

    private float m_initialSpeed;
    private float m_deceleration;
    
    private PlayerMotor m_motor;
    private Launchable m_launchable;
    private PlayerAim m_aim;

    private Coroutine m_invincibilityCountdown;
    private Coroutine m_unlockCountdown;
    private float m_oldRadius;

    public void Awake() {
        m_motor = GetComponent<PlayerMotor>();
        m_launchable = GetComponent<Launchable>();
        m_aim = GetComponent<PlayerAim>();
    }

    public override void Press() {
        m_initialSpeed = 2f * ShortDashDistance / ShortDashTime;
        m_deceleration = m_initialSpeed / ShortDashTime;

        Debug.Log("Dash {Speed, Deceleration} = { " + m_initialSpeed + ", " + m_deceleration + " }");
    }

    public override void Hold() {
        m_chargeTime += Time.deltaTime;

        if (m_chargeTime > LongDashChargeThreshold) {
            m_motor.Movement = PlayerMotor.MovementStyle.Pivot;
            
            m_oldRadius = m_aim.OuterRadius;
            m_aim.OuterRadius = Radius;
        }
    }

    public override void Release() {
        //m_aim.Hide();
        if (m_chargeTime > LongDashChargeThreshold) {
            if (m_motor.WarpTo(m_aim.Target, Radius)) {
                m_motor.Movement = PlayerMotor.MovementStyle.Free;
            }
            m_aim.OuterRadius = m_oldRadius;
            m_aim.Recenter();
            // Fall through to standard dash?
        } else {
            if (m_motor.Velocity == Vector3.zero) {
                m_launchable.Launch(m_aim.Direction * m_initialSpeed, m_deceleration, 0);
            } else {
                m_launchable.Launch(m_motor.Direction * m_initialSpeed, m_deceleration, 0);
            }

            if (m_invincibilityCountdown != null) {
                StopCoroutine(m_invincibilityCountdown);
            }
            m_invincibilityCountdown = StartCoroutine(InvincibilityCountdown(InvincibilityTime));

            if (m_unlockCountdown != null) {
                StopCoroutine(m_unlockCountdown);
            }
            m_unlockCountdown = StartCoroutine(UnlockCountdown(FreezeMovmentControlTime));
        }

        m_chargeTime = 0f;
    }

    private IEnumerator InvincibilityCountdown(float time) {
        yield return new WaitForSeconds(time);
    }

    private IEnumerator UnlockCountdown(float time) {
        yield return new WaitForSeconds(time);
        m_motor.Movement = PlayerMotor.MovementStyle.Free;
    }
}