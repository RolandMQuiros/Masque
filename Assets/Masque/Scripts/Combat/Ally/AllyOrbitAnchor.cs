using UnityEngine;
using System.Collections;

public class AllyOrbitAnchor : MonoBehaviour {
    public Vector3 Offset;
    public Vector3 Up = Vector3.up;

    private NavMeshAgent m_agent;

    public Vector3 m_worldOffset;
    public Vector3 m_edge;
    public Vector3 m_projection;
    public Vector3 m_groundNormal;
    public NavMeshHit m_hit;
    public bool raycast;
    public Vector3 point;
    
    public void Start() {
        if (transform.parent != null) {
            m_agent = transform.parent.gameObject.GetComponent<NavMeshAgent>();
        }
    }

	public void Update () {
        if (transform.parent != null) {
            m_worldOffset = transform.parent.TransformPoint(Offset);

            // TODO: Incorporate incline
            //RaycastHit normalHit;
            //raycast = Physics.Raycast(m_worldOffset + m_plane.Up, -m_plane.Up, out normalHit);
            //m_groundNormal = normalHit.normal;
            //point = normalHit.point;

            // Start the raycast from the opposite end of the NavMeshAgent's cylinder.  This prevents the raycast
            // origin and the intersection point from being equal, and resulting in a zero normal.
            Vector3 direction = (transform.parent.position - m_worldOffset).normalized;
            Vector3 startCast = transform.parent.position + (m_agent.radius * direction);

            if (NavMesh.Raycast(startCast, m_worldOffset, out m_hit, NavMesh.AllAreas)) {
                m_edge = Vector3.Cross(m_hit.normal, Up);

                m_projection = Vector3.Project(m_worldOffset - transform.parent.position, m_edge);

                transform.position = transform.parent.position +
                                     Vector3.Project(m_hit.position - transform.parent.position, m_hit.normal) +
                                     m_projection;
            } else {
                transform.localPosition = Offset;
            }
        }
	}

    public void OnDrawGizmos() {
        if (isActiveAndEnabled) {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(transform.position, Vector3.one);

            if (transform.parent != null) {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.parent.position, m_worldOffset);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(m_worldOffset, m_worldOffset + m_hit.normal);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(m_worldOffset, m_worldOffset + m_edge);

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(m_hit.position, 0.3f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(m_worldOffset, m_worldOffset + m_groundNormal);
        }
    }
}
