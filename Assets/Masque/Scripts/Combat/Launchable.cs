using UnityEngine;
using System;
using System.Collections;

public class BounceEventArgs : EventArgs {
    public Vector3 Incident;
    public Vector3 Normal;
    public Vector3 Reflection;
}

[RequireComponent(typeof(Combatant))]
[RequireComponent(typeof(NavMeshAgent))]
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

    private NavMeshAgent m_navMeshAgent;
    private ParticleSystem m_particles;

    private Vector3 m_velocity;
    private float m_speed;
    private int m_bounces;
    private float m_initSlopeLimit;
    private float m_initStepOffset;

	public void Awake () {
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        m_particles = GetComponent<ParticleSystem>();
	}

    public void Launch(Vector3 initialVelocity, float deceleration, int bounces = -1) {
        IsLaunched = true;

        if (m_particles) {
            m_particles.Play();
        }

        m_bounces = bounces;
        StartCoroutine(ProcessLaunch(initialVelocity, deceleration, bounces));
    }

    public void LaunchForward(float initialSpeed, float deceleration, int bounces = -1) {
        Vector3 initialVelocity = initialSpeed * (transform.rotation * Vector3.forward);
        Launch(initialVelocity, deceleration, bounces);
    }

    public void LaunchBackward(float initialSpeed, float deceleration, int bounces = -1) {
        Vector3 initialVelocity = initialSpeed * (transform.rotation * Vector3.back);
        Launch(initialVelocity, deceleration, bounces);
    }

    public void InterruptLaunch(bool cancelMomentum = false) {
        if (IsLaunched) {
            IsLaunched = false;

            if (cancelMomentum) {
                m_speed = 0f;
                m_velocity = Vector3.zero;
            }
        }
    }

    private IEnumerator ProcessLaunch(Vector3 initialVelocity, float deceleration, int bounces = -1) {
        Vector3 velocity = initialVelocity;
        float speed = velocity.magnitude;

        while (speed > 0f) {
            float dt = Time.deltaTime;

            if (m_navMeshAgent.enabled) {
                NavMeshHit hit;
                bool collision = m_navMeshAgent.Raycast(transform.position + velocity * dt, out hit);

                if (collision) {
                    if (bounces != 0) {
                        
                        Vector3 reflection = Vector3.Reflect(velocity, hit.normal);

                        bounces--;
                        if (Bounced != null) {
                            Bounced(this, new BounceEventArgs {
                                Incident = velocity,
                                Normal = hit.normal,
                                Reflection = reflection
                            });
                        }
                        velocity = reflection;
                    }
                }
                
                m_navMeshAgent.Move(velocity * dt);
            }

            speed -= deceleration * dt * dt;
            velocity -= deceleration * dt * dt * velocity.normalized;

            yield return null;
        }

        IsLaunched = false;
    }
}
