using UnityEngine;
using System.Collections;

public class BoltSkill : PlayerSkill {
    [Tooltip("How long the button needs to be held before charging begins")]
    public float ChargeStartThreshold = 0.25f;
    [Tooltip("How long the button needs to be held before a charged bolt can be fired")]
    public float ChargeHoldThreshold = 1f;
    [Tooltip("The time after a quick bolt is thrown that the player is locked in-place")]
    public float StunBoltLockTime = 1f;

    [Tooltip("The time after a quick bolt is thrown that the player is locked in strafing movement")]
    public float QuickBoltLockTime = 0.4f;
    [Tooltip("Time between quick bolt shots")]
    public float QuickBoltDelay = 0.1f;
    [Tooltip("Maximum number of quick bolts allowed in a flurry")]
    public int FlurryMaxBolts = 3;
    [Tooltip("Number of quick bolts needed to be fired in succession before a quick bolt flurry can start")]
    public int FlurryPressThreshold = 4;
    [Tooltip("Time after press to scan for a flurry input, that is, the number of presses indicated by Flurry Press " +
             "Threshold")]
    public float FlurryPressTime = 0.2f;
    public float FlurryLockTime = 1f;

    [Tooltip("How fast the player can move while strafing")]
    public float AimingMovementSpeed = 40f;
    public float QuickBoltSpread = 30f;

    public Bullet QuickBoltPrefab;
    public Bullet StunBoltPrefab;

    private float m_chargeTime;
    private int m_quickBolts = 0;

    private Coroutine m_flurryScan;
    private Coroutine m_flurry;
    private Coroutine m_cooldown;

    private bool m_isFlurryScanning = false;
    private bool m_canFlurry = false;
    private bool m_isFlurrying = false;

    private PlayerMotor m_motor;
    private MotionBuffer m_motion;
    private PlayerAim m_aim;
    private Launchable m_launchable;
    
    public void Awake() {
        QuickBoltPrefab.CreatePool(10);
        StunBoltPrefab.CreatePool(2);

        m_motor = GetComponent<PlayerMotor>();
        m_motion = GetComponent<MotionBuffer>();
        m_aim = GetComponent<PlayerAim>();
        m_launchable = GetComponent<Launchable>();
    }

    private IEnumerator CooldownBeforeUnlockingPlayer(float time) {
        yield return new WaitForSeconds(time);
        m_motor.Movement = PlayerMotor.MovementStyle.Free;
        IsFinished = true;
    }

    private IEnumerator Flurry() {
        m_isFlurrying = true;
        m_motor.Movement = PlayerMotor.MovementStyle.Lock;
        Quaternion rotation = m_aim.Rotation;

        // Fire bullets with a time delay
        while (m_canFlurry) {
            FireQuickBolt(rotation);
            m_motion.Move(rotation * (Vector3.forward * 5f * Time.deltaTime));
            yield return new WaitForSeconds(QuickBoltDelay);
        }
        m_isFlurrying = false;
        IsFinished = true;
    }

    private IEnumerator FlurryScanCountdown(float time) {
        m_isFlurryScanning = true;

        // This coroutine is started right after the first quick bolt is tossed
        int quickBolts = m_quickBolts;
        yield return new WaitForSeconds(time);
        
        // If the number of quickbolts tossed after the elapsed time is the same as before, then the player is clearly
        // not rapidly pressing the button, and the flurry scan fails.
        if (quickBolts == m_quickBolts) {
            m_canFlurry = false;
            m_quickBolts = 0;
            m_motor.Movement = PlayerMotor.MovementStyle.Free;
        }

        m_isFlurryScanning = false;
    }

    private void FireQuickBolt(Quaternion rotation) {
        // Calculate spread
        float spread = 2f * (Random.value - 0.5f) * QuickBoltSpread;
        
        Quaternion direction = rotation * Quaternion.AngleAxis(spread, Vector3.up);

        // Fire quick bolt bullet
        QuickBoltPrefab.Spawn(transform.position, direction);
    }

    public override void Press() {
        IsFinished = false;
    }

    public override void Hold() {
        m_chargeTime += Time.deltaTime;

        if (m_chargeTime > ChargeStartThreshold) {
            m_motor.Movement = PlayerMotor.MovementStyle.Pivot;
        }
    }
    public override void Release() {
        // If charge above the Stun threshold, toss a long-range pinning bolt
        if (m_chargeTime > ChargeHoldThreshold) {
            StunBoltPrefab.Spawn(transform.position, transform.rotation);
            m_launchable.LaunchBackward(40f, 80f, 0);

            // Lock player input for a little while
            if (m_cooldown != null) {
                StopCoroutine(m_cooldown);
            }
            m_cooldown = StartCoroutine(CooldownBeforeUnlockingPlayer(StunBoltLockTime));
            // Otherwise, toss a short range stunning bolt
        } else {
            // Check if a number of quick bolts have been tossed in the last few moments.  If so, looks like the player
            // is trying to flurry.
            m_quickBolts++;

            // Start flurry scan
            if (m_flurryScan != null) {
                StopCoroutine(m_flurryScan);
            }
            m_flurryScan = StartCoroutine(FlurryScanCountdown(FlurryPressTime));

            // Only flurry while not already flurrying, flurreal
            if (!m_isFlurrying) {
                if (m_quickBolts > FlurryPressThreshold) {
                    m_canFlurry = true;
                    m_flurry = StartCoroutine(Flurry());
                    // Otherwise, just toss out a single quick bolt while constraining the player to strafing movement
                } else {
                    // Spawn single bullet
                    FireQuickBolt(m_aim.Rotation);
                    m_launchable.LaunchForward(5f, 20f, 0);

                    // Keep the player strafing for a little while
                    if (m_cooldown != null) {
                        StopCoroutine(m_cooldown);
                    }
                    m_cooldown = StartCoroutine(CooldownBeforeUnlockingPlayer(QuickBoltLockTime));
                }
            }
        }
        m_chargeTime = 0f;
    }

    public override bool Interrupt() {
        StopAllCoroutines();
        m_isFlurrying = false;
        m_isFlurryScanning = false;
        m_canFlurry = false;
        m_quickBolts = 0;

        return true;
    }

    public override bool Interrupt(PlayerSkill other) {
        if (other is DashSkill) {
            Interrupt();
            return true;
        }
        return false;
    }
}
