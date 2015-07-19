using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MotionBuffer))]
public class DeferredFollower : MonoBehaviour {
    [Tooltip("Target GameObject's Transform")]
    public Transform Target;
    [Tooltip("Time between Linecasts between this object and its Target.  A failed raycast causes this Follower " +
             "to use pathfinding instead of direct movement")]
    public float LinecastInterval = 2f;
    [Tooltip("Length of the Linecast")]
    public float LinecastLength = 10f;
    public float Speed = 100f;

    private MotionBuffer m_motion;
    private Coroutine m_linecaster;
    private bool m_isTargetObscured = false;

	public void Awake () {
        m_motion = GetComponent<MotionBuffer>();
        m_linecaster = StartCoroutine(CheckLinecast());
	}

    public void OnEnable() {
        m_linecaster = StartCoroutine(CheckLinecast());
    }

    public void OnDisable() {
        StopCoroutine(m_linecaster);
    }
	
	public void Update () {
        if (Target.gameObject.activeSelf) {
            /*if (m_isTargetObscured) {
                m_motion.Agent.SetDestination(Target.position);
            } else {*/
                Vector3 dp = Target.position - transform.position;
                Vector3 direction = dp.normalized;
                Vector3 move = direction * Speed * Time.deltaTime;

                if (dp.sqrMagnitude < move.sqrMagnitude) {
                    //m_motion.Move(dp);
                    transform.position += dp;
                } else {
                    //m_motion.Move(move);
                    transform.position += move;
                }
            //}
        }
	}

    public void OnDrawGizmos() {
        if (Target != null && Target.gameObject.activeSelf) {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(Target.position, 0.2f);
        }
    }

    private IEnumerator CheckLinecast() {
        while (true) {
            if (Target != null && Target.gameObject.activeSelf) {
                NavMeshHit hit;
                m_isTargetObscured = m_motion.Agent.Raycast(Target.position, out hit);
            }

            yield return new WaitForSeconds(LinecastInterval);
        }
    }
}
