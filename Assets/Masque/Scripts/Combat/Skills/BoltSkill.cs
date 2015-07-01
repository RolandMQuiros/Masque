using UnityEngine;
using System.Collections;

public class BoltSkill : PlayerSkill {
    private const float CHARGE_START_THRESH = 0.25f;
    private const float CHARGE_HOLD_THRESH = 1f;
    private const float QUICKBOLT_LOCK_TIME = 0.1f;

    public float AimingMovementSpeed = 40f;
    public float QuickBoltSpread = 30f;
    public float StunBoltRecoveryTime = 1f;

    private Bullet m_quickBolt;
    private Bullet m_stunBolt;
    
    private float m_chargeTime;
    private float m_quickLockDuration;
    private float m_stunRecoveryDuration;
    private bool m_isQuickBoltLocked;
    private bool m_isStunBoltLocked;
    private PlayerController m_controller;

    public BoltSkill(GameObject owner, Bullet quickBoltPrefab, Bullet stunBoltPrefab) :
    base(owner) {
        m_quickBolt = quickBoltPrefab;
        m_stunBolt = stunBoltPrefab;
        
        m_quickBolt.CreatePool(10);
        m_stunBolt.CreatePool(2);

        m_controller = Owner.GetComponent<PlayerController>();
    }

    public override void Start() {
    }

    public override void Hold() {
        m_controller.Movement = PlayerController.MovementStyle.Lock;
        m_chargeTime += Time.deltaTime;

        if (m_chargeTime > CHARGE_START_THRESH) {
            m_controller.Movement = PlayerController.MovementStyle.Strafe;
        }
    }
    public override void Release() {
        if (m_chargeTime > CHARGE_HOLD_THRESH) {
            // If charge above the Stun threshold, toss a long-range pinning bolt
            m_stunBolt.Spawn(Owner.transform.position, Owner.transform.rotation);
            m_isStunBoltLocked = true;

            m_controller.NudgeBackward(10f, 2000f);

        } else {
            // Otherwise, toss a short range stunning bolt
            m_controller.Movement = PlayerController.MovementStyle.Lock;            

            // Calculate spread
            float spread = 2f * (Random.value - 0.5f) * QuickBoltSpread;
            Quaternion direction = Owner.transform.rotation * Quaternion.AngleAxis(spread, Vector3.up);

            // Spawn bullet
            m_quickBolt.Spawn(Owner.transform.position, direction);
            m_quickLockDuration = QUICKBOLT_LOCK_TIME;
            m_isQuickBoltLocked = true;

            m_controller.NudgeBackward(5f, 2000f);
        }
        m_chargeTime = 0f;
    }

    public override void Update() {
        if (m_isQuickBoltLocked) {
            if (m_quickLockDuration > 0f) {
                m_quickLockDuration -= Time.deltaTime;
            } else {
                m_isQuickBoltLocked = false;
                m_controller.Movement = PlayerController.MovementStyle.Run;
            }
        }

        if (m_isStunBoltLocked) {
            if (m_stunRecoveryDuration > 0f) {
                m_stunRecoveryDuration -= Time.deltaTime;
            } else {
                m_isStunBoltLocked = false;
                m_controller.Movement = PlayerController.MovementStyle.Run;
            }
        }
    }
}
