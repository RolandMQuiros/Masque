using UnityEngine;
using System.Collections;

/// <summary>
/// Wrapper for NavMeshAgent that collects and buffers all movements by other components into a single call to
/// NavMeshAgent.Move.  Saves cycles on collision/NavMesh detection.
/// 
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class MotionBuffer : MonoBehaviour {
    public Vector3 Offset;
    public Vector3 DebugOffset;
    public NavMeshAgent Agent { get; private set; }

    private Coroutine m_updater;

	public void Awake() {
        Agent = GetComponent<NavMeshAgent>();
	}

    public void Start() {
        m_updater = StartCoroutine(BeforeLateUpdate());
    }

    public void OnEnable() {
        m_updater = StartCoroutine(BeforeLateUpdate());
    }

    public void OnDisable() {
        StopCoroutine(m_updater);
    }

    public bool Warp(Vector3 newPosition) {
        return Agent.Warp(newPosition);
    }

    public void Move(Vector3 offset) {
        Offset += offset;
    }
    
    /// <summary>
    /// Applies offset vector. This is implemented as coroutine in order to run the update between GameObject Updates
    /// and LateUpdates, so movement doesn't disrupt Camera movement, which occurs in LateUpdate.
    /// </summary>
    /// <returns></returns>
    private IEnumerator BeforeLateUpdate() {
        while (true) {
            if (Agent != null && Agent.enabled) {
                Agent.Move(Offset);
            } else {
                transform.Translate(Offset);
            }

            DebugOffset = Offset;
            Offset = new Vector3();
            yield return new WaitForEndOfFrame();
        }
    }
}
