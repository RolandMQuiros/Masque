using UnityEngine;
using System.Collections;

public class DashSkill : PlayerSkill {
    public float LongDashChargeThreshold = 0.1f;
    public float ShortDashSpeed = 80f;
    public float ShortDashDeceleration = 1000f;
    public float FreezeMovmentControlTime = 0.2f;
    public float InvincibilityTime = 0.5f;

    public float Radius = 20f;

    private float m_chargeTime = 0f;
    
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
                m_launchable.Launch(m_aim.Direction * ShortDashSpeed, ShortDashDeceleration, 0);
                Debug.Log(m_motor.Velocity.ToString() + " Aim Dash");
            } else {
                m_launchable.Launch(m_motor.Direction * ShortDashSpeed, ShortDashDeceleration, 0);
                Debug.Log(m_motor.Velocity.ToString() + "Motor Dash");
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
        Debug.Log("Dash invincibility start");
        yield return new WaitForSeconds(time);
        Debug.Log("Dash invincibility end");
    }

    private IEnumerator UnlockCountdown(float time) {
        yield return new WaitForSeconds(time);
        m_motor.Movement = PlayerMotor.MovementStyle.Free;
    }
}