using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Combatant))]
public class MinonNav : MonoBehaviour {
    public enum State {
        Approach,
        Attack
    }

    public Bullet BulletPrefab;
    public Transform PrimaryTarget;
    public Transform[] SecondaryTargets;
    public float AttackCooldown = 1f;
    public float AttackDistance = 10f;

    public State DebugState;

    private NavMeshAgent m_navAgent;
    private Combatant m_combatant;

    private State m_state;
    private float m_cooldownTimer = 0f;
    private Transform m_target;
    
	// Use this for initialization
	public void Awake() {
        m_navAgent = GetComponent<NavMeshAgent>();
        m_combatant = GetComponent<Combatant>();
	}
	
	// Update is called once per frame
	public void Update() {
        switch (m_state) {
            case State.Approach:
                m_state = UpdateApproach();
                break;
            case State.Attack:
                m_state = UpdateAttack();
                break;
        }
        DebugState = m_state;
	}

    private State UpdateApproach() {
        State nextState = State.Approach;

        if (WithinDistanceToATarget()) {
            nextState = State.Attack;
        }

        return nextState;
    }

    private State UpdateAttack() {
        State nextState = State.Attack;

        Vector3 offset = m_target.position - transform.position;
        offset.y = 0f;

        transform.rotation = Quaternion.LookRotation(offset, Vector3.up);

        m_cooldownTimer += Time.deltaTime;
        if (m_cooldownTimer > AttackCooldown) {
            BulletPrefab.Spawn(transform.position, transform.rotation);
            m_cooldownTimer = 0f;
        }

        if (!WithinDistanceToATarget()) {
            nextState = State.Approach;
        }

        return nextState;
    }

    private bool WithinDistanceToATarget() {
        bool found = false;


        // Need to reset NavMeshAgent.remainingDistance for the calculation in WithinDistanceToATarget
        if (!m_navAgent.pathPending && PrimaryTarget.transform.hasChanged) {
            m_navAgent.SetDestination(PrimaryTarget.position);
        }

        float distance = m_navAgent.remainingDistance;
        found = (m_navAgent.remainingDistance <= m_navAgent.stoppingDistance) &&    // Check if within distance
                (!m_navAgent.hasPath || m_navAgent.velocity.sqrMagnitude == 0f);    // Check if steering is finished

        if (found) {
            m_target = PrimaryTarget;
        } else {
            for (int i = 0; i < SecondaryTargets.Length && !found; i++) {
                distance = Vector3.Distance(SecondaryTargets[i].position, transform.position);
                found = distance < AttackDistance;
                m_target = SecondaryTargets[i];
            }
        }

        return found;
    }
}
