using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAllyOrbiter : MonoBehaviour {
    public float Radius = 4f;
    public DeferredFollower ActiveAlly {
        get {
            if (m_allies.Count > 0) {
                return m_allies.Get(0);
            } else {
                return null;
            }
        }
    }

    private List<Vector3> m_offsets = new List<Vector3>();
    private Deque<DeferredFollower> m_allies = new Deque<DeferredFollower>();
    private List<AllyOrbitAnchor> m_anchors = new List<AllyOrbitAnchor>();

    public void Start() {
        UpdateOffsets();
    }

    public void Update() {
        for (int i = 0; i < m_allies.Count; i++) {
            if (i == 0) {
                m_allies[i].transform.localRotation = Quaternion.identity;
            }

            m_allies[i].Target = m_anchors[i].transform.position;
        }
    }
    
    public void AddAlly(DeferredFollower ally) {
        if (ally != null) {
            m_allies.AddBack(ally);
            UpdateOffsets();
        }
    }

    public void RemoveAlly(DeferredFollower ally) {
        if (ally != null) {
            m_allies.Remove(ally);
            OrphanAlly(ally);

            UpdateOffsets();
        }
    }

    public void RemoveFront() {
        DeferredFollower front = m_allies.RemoveFront();
        OrphanAlly(front);

        UpdateOffsets();
    }

    public void RemoveBack() {
        m_allies.RemoveBack();
        UpdateOffsets();
    }

    public void CycleForward() {
        if (m_allies.Count > 0) {
            DeferredFollower front = m_allies.RemoveFront();
            OrphanAlly(front);

            m_allies.AddBack(front);
            AdoptAlly(m_allies.Get(0));
        }
    }

    public void CycleBackward() {
        if (m_allies.Count > 0) {
            DeferredFollower back = m_allies.RemoveBack();
            DeferredFollower front = m_allies.Get(0);

            OrphanAlly(front);
            m_allies.AddFront(back);
            AdoptAlly(back);
        }
    }

    public void UpdateOffsets() {
        float angleInterval = 2f * Mathf.PI / m_allies.Count;

        // Disable any extra anchor objects
        for (int i = 0; i < m_anchors.Count; i++) {
            if (i < m_allies.Count) {
                m_anchors[i].gameObject.SetActive(true);
            } else {
                m_anchors[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < m_allies.Count; i++) {
            // Calculate offset
            float angleOffset = angleInterval * i;
            Vector3 offset = new Vector3(
                Radius * Mathf.Sin(angleOffset),
                0f,
                Radius * Mathf.Cos(angleOffset)
                );

            AllyOrbitAnchor anchor;
            if (i < m_anchors.Count) {
                // If the anchor object already exists, reassign its position
                m_anchors[i].gameObject.SetActive(true);
                anchor = m_anchors[i].gameObject.GetComponent<AllyOrbitAnchor>();
                anchor.Offset = offset;

                m_offsets[i] = offset;
            } else {
                // Otherwise, instantiate a new anchor object and add it to the list
                GameObject child = new GameObject("Sys_Ally_Anchor_" + i);
                child.transform.parent = gameObject.transform;

                anchor = child.AddComponent<AllyOrbitAnchor>() as AllyOrbitAnchor;
                anchor.Offset = offset;

                m_anchors.Add(anchor);
                m_offsets.Add(offset);
            }

            // Point each ally to their respective point
            m_allies[i].Target = m_anchors[i].transform.position;

            if (i == 0) {
                AdoptAlly(m_allies[i]);
            }
        }
    }

    private void AdoptAlly(DeferredFollower ally) {
        ally.transform.parent = gameObject.transform;
        ally.RotateToMovement = false;
        ally.transform.localRotation = Quaternion.identity;
    }

    private void OrphanAlly(DeferredFollower ally) {
        ally.transform.parent = null;
        ally.RotateToMovement = true;
    }
}
