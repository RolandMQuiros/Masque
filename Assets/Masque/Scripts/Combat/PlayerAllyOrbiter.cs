using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAllyOrbiter : MonoBehaviour {
    public float Radius = 4f;
    public DeferredFollower[] Allies = new DeferredFollower[1];
    public DeferredFollower ActiveAlly {
        get {
            return Allies[(int)m_startIdx];
        }
    }
    
    private List<Vector3> m_offsets = new List<Vector3>();
    private List<DeferredFollower> m_allies = new List<DeferredFollower>();
    private List<AllyOrbitAnchor> m_anchors = new List<AllyOrbitAnchor>();

    private uint m_startIdx = 0;

    public void Start() {
        for (int i = 0; i < Allies.Length; i++) {
            m_allies.Add(Allies[i]);
        }

        UpdateOffsets();
    }

    public void Update() {
        if (Input.GetButtonDown("Cycle Forward")) {
            CycleForward();
        } else if (Input.GetButtonDown("Cycle Backward")) {
            CycleBackward();
        }

        for (int i = 0; i < m_allies.Count; i++) {
            m_allies[((int)m_startIdx + i) % m_allies.Count].Target = m_anchors[i].transform.position;
        }
    }
    
    public void AddAlly(DeferredFollower ally) {
        m_allies.Add(ally);
        UpdateOffsets();
    }

    public void RemoveAlly(DeferredFollower ally) {
        int idx = m_allies.IndexOf(ally);
        if (idx >= 0) {
            m_allies.RemoveAt(idx);
            UpdateOffsets();
        }
    }

    public void CycleForward() {
        m_startIdx = (m_startIdx - 1) % (uint)m_allies.Count;
        for (int i = 0; i < m_allies.Count; i++) {
            int idx = (int)(m_startIdx + i) % m_allies.Count;
        }
    }

    public void CycleBackward() {
        m_startIdx = (m_startIdx + 1) % (uint)m_allies.Count;
        for (int i = 0; i < m_allies.Count; i++) {
            int idx = (int)(m_startIdx + i) % m_allies.Count;
        }
    }

    public void UpdateOffsets() {
        float angleInterval = 2f * Mathf.PI / m_allies.Count;

        // Disable any extra anchor objects
        for (int i = m_allies.Count; i < m_anchors.Count; i++) {
            m_anchors[i].gameObject.SetActive(false);
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
            m_allies[((int)m_startIdx + i) % m_allies.Count].Target = m_anchors[i].transform.position;
        }
    }
}
