using UnityEngine;
using System.Collections;

/// <summary>
/// A Character controller that uses Rigidbodies
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class RigidController : MonoBehaviour {
    public Vector3 Velocity {
        get { return m_velocity;  }
    }
    public float MaxVelocityChange = 10f;

    private Rigidbody m_rigidbody;
    private Collider m_collider;

    private Vector3 m_velocity;
    private bool m_moveConsumed = false;

	// Use this for initialization
	public void Awake() {
        m_rigidbody = GetComponent<Rigidbody>();
        m_collider = GetComponent<Collider>();
	}

    public void Move(Vector3 velocity) {
        m_velocity = velocity;
        m_moveConsumed = false;
    }

	public void FixedUpdate() {
        Vector3 rigidVelocity = m_rigidbody.velocity;
        Vector3 dv = (m_velocity - rigidVelocity);

        dv.x = Mathf.Clamp(dv.x, -MaxVelocityChange, MaxVelocityChange);
        dv.z = Mathf.Clamp(dv.z, -MaxVelocityChange, MaxVelocityChange);
        dv.y = 0f;

        m_rigidbody.AddForce(dv, ForceMode.VelocityChange);
        m_velocity = Vector3.zero;
	}
}
