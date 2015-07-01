using UnityEngine;
using System;
using System.Collections;

public class BounceEventArgs : EventArgs {
    public Vector3 Incident;
    public Vector3 Normal;
    public Vector3 Reflection;
}

[RequireComponent(typeof(Combatant))]
[RequireComponent(typeof(CharacterController))]
public class Launchable : MonoBehaviour {
    /// <summary>Signal that emits when this Launchable bounces on a wall</summary>
    public EventHandler<BounceEventArgs> Bounced;
    /// <summary>
    /// Flag indicating if this Launchable is currently Launched.  A Launchable can only support a single launch at a
    /// time.
    /// </summary>
    public bool IsLaunched {
        get;
        private set;
    }
    /// <summary>Which layers are checked for bouncing</summary>
    [Tooltip("Which layers are checked for bounces")]
    public LayerMask BounceLayers;

    private CharacterController m_controller;
    private Combatant m_combatant;
    private ParticleSystem m_particles;

    private Vector3 m_velocity;
    private float m_speed;
    private int m_bounces;
    private float m_initSlopeLimit;
    private float m_initStepOffset;

	public void Awake () {
        m_controller = GetComponent<CharacterController>();
        m_combatant = GetComponent<Combatant>();
        m_particles = GetComponent<ParticleSystem>();
	}

    public void Launch(Vector3 initialVelocity, float deceleration, int bounces = -1) {
        if (!IsLaunched) {
            IsLaunched = true;

            if (m_particles) {
                m_particles.Play();
            }

            // Essentially turn off slope limiting so the CharacterController doesn't get
            // caught on anything
            m_initSlopeLimit = m_controller.slopeLimit;
            m_initStepOffset = m_controller.stepOffset;

            m_controller.slopeLimit = 0f;
            m_controller.stepOffset = m_controller.height;

            m_bounces = bounces;
            StartCoroutine(ProcessLaunch(initialVelocity, deceleration));
        }
    }

    public void InterruptLaunch(bool cancelMomentum = false) {
        if (IsLaunched) {
            m_controller.slopeLimit = m_initSlopeLimit;
            m_controller.stepOffset = m_initStepOffset;
            IsLaunched = false;

            if (cancelMomentum) {
                m_speed = 0f;
                m_velocity = Vector3.zero;
            }
        }
    }
    
    public void OnControllerColliderHit(ControllerColliderHit hit) {
        Debug.Log("Bounce!");
        if (m_bounces != 0) {
            Vector3 reflection = Vector3.Reflect(m_velocity, hit.normal);
            
            m_bounces--;
            if (Bounced != null) {
                Bounced(this, new BounceEventArgs {
                    Incident = m_velocity,
                    Normal = hit.normal,
                    Reflection = reflection
                });
            }
            m_velocity = reflection;
        }
    }

    private IEnumerator ProcessLaunch(Vector3 initialVelocity, float deceleration) {
        m_velocity = initialVelocity;
        m_speed = m_velocity.magnitude;

        while (m_speed > 0f) {
            float dt = Time.deltaTime;

            if (m_controller.enabled) {
                m_controller.Move(m_velocity * dt);
            }

            m_speed -= deceleration * dt * dt;
            m_velocity -= deceleration * dt * dt * m_velocity.normalized;

            yield return null;
        }

        m_controller.slopeLimit = m_initSlopeLimit;
        m_controller.stepOffset = m_initStepOffset;
        IsLaunched = false;
    }
}
