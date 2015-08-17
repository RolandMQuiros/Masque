using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MotionBuffer))]
public class DeferredFollower : MonoBehaviour {
    private const float ROTATE_TO_MOVE_THRESHOLD = 1f;

    [Tooltip("Target position")]
    public Vector3 Target;
    [Tooltip("Time between Linecasts between this object and its Target.  A failed raycast causes this Follower " +
             "to use pathfinding instead of direct movement")]
    public float LinecastInterval = 2f;
    [Tooltip("Length of the Linecast")]
    public float LinecastLength = 10f;

    public bool RotateToMovement = true;

    private MotionBuffer m_motion;
    private NavMeshAgent m_agent;
    private Coroutine m_linecaster;

    public bool m_isTargetObscured = false;
    public bool m_usePathfinding = false;
    public bool m_linecasterRunning = false;

    public Vector3 dest;
    public float dip;

	public void Awake () {
        m_motion = GetComponent<MotionBuffer>();
        m_agent = GetComponent<NavMeshAgent>();
	}

    public void OnEnable() {
        if (m_linecaster == null) {
            m_linecaster = StartCoroutine(CheckLinecast());
        }
    }

    public void OnDisable() {
        if (m_linecaster != null) {
            StopCoroutine(m_linecaster);
        }
    }

    public void ForcePathfinding() {
        if (m_linecaster != null) {
            StopCoroutine(m_linecaster);
        }
        m_linecasterRunning = false;
    }
	
	public void Update () {
        NavMeshHit hit;
        m_isTargetObscured = m_agent.Raycast(Target, out hit);
        
        if (m_isTargetObscured && !m_linecasterRunning) {
            m_linecaster = StartCoroutine(CheckLinecast());
        } else if (!m_isTargetObscured && m_linecasterRunning) {
            m_linecasterRunning = false;
            m_usePathfinding = false;
            m_agent.ResetPath();
            StopCoroutine(m_linecaster);
        } else {
            m_agent.ResetPath();
            m_linecasterRunning = false;
            m_usePathfinding = false;
        }

        if (!m_usePathfinding) {
            NavMesh.SamplePosition(Target, out hit, m_agent.height, NavMesh.AllAreas);
            
            Vector3 dp = hit.position - transform.position;
            Vector3 direction = dp.normalized;
            Vector3 move = direction * m_agent.speed * Time.deltaTime;

            dest = hit.position;

            //if (dp.sqrMagnitude < move.sqrMagnitude) {
            //    m_motion.Move(dp);
            //} else {
                m_motion.Move(move);
            //}

            dip = dp.magnitude;

            if (RotateToMovement && dp.magnitude > ROTATE_TO_MOVE_THRESHOLD) {
                Quaternion rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(direction, Vector3.up), Vector3.up);
                Quaternion newRotation = Quaternion.Lerp(transform.rotation, rot, 10f * Time.deltaTime);
                transform.rotation = newRotation;
            }
        }
	}

    public void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(Target, 0.2f);
    }

    private IEnumerator CheckLinecast() {
        m_linecasterRunning = true;
        while (m_linecasterRunning) {
            yield return new WaitForSeconds(LinecastInterval);

            m_usePathfinding = m_isTargetObscured;
            
            if (m_isTargetObscured) {
                m_agent.SetDestination(Target);
            } else {
                m_agent.ResetPath();
            }
        }
    }
}
