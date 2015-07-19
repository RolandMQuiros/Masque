using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAllyOrbiter : MonoBehaviour {
    public float Radius = 4f;
    public DeferredFollower[] Allies = new DeferredFollower[1];

    private PlanarViewVectors m_plane;

    private List<DeferredFollower> m_allies = new List<DeferredFollower>();
    private List<GameObject> m_offsets = new List<GameObject>();

    private uint m_startIdx = 0;

	// Use this for initialization
	public void Awake() {
        m_plane = GetComponent<PlanarViewVectors>();
	}

    public void Start() {
        for (int i = 0; i < Allies.Length; i++) {
            m_allies.Add(Allies[i]);
        }

        UpdateOffsets();

        for (int i = 0; i < m_allies.Count; i++) {
            m_allies[i].Target = m_offsets[i].transform;
        }
    }

    public void Update() {
        if (Input.GetButtonDown("Cycle Forward")) {
            CycleForward();
        } else if (Input.GetButtonDown("Cycle Backward")) {
            CycleBackward();
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
        m_startIdx = (m_startIdx + 1) % (uint)m_allies.Count;
        for (int i = 0; i < m_allies.Count; i++) {
            int idx = (int)(m_startIdx + i) % m_allies.Count;
            m_allies[idx].Target = m_offsets[i].transform;
        }
    }

    public void CycleBackward() {
        m_startIdx = (m_startIdx - 1) % (uint)m_allies.Count;
        for (int i = 0; i < m_allies.Count; i++) {
            int idx = (int)(m_startIdx + i) % m_allies.Count;
            m_allies[idx].Target = m_offsets[i].transform;
        }
    }

    public void UpdateOffsets() {
        float angleInterval = 2f * Mathf.PI / m_allies.Count;
        GameObject empty = new GameObject();

        // Disable any additional offset objects
        for (int i = m_allies.Count; i < m_offsets.Count; i++) {
            m_offsets[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < m_allies.Count; i++) {
            // Calculate offset
            float angleOffset = angleInterval * i;
            Vector3 offset = new Vector3(
                Radius * Mathf.Cos(angleOffset),
                0f,
                Radius * Mathf.Sin(angleOffset)
                );

            if (i < m_offsets.Count) {
                // If the offset object already exists, reassign its position
                m_offsets[i].gameObject.SetActive(true);
                m_offsets[i].transform.localPosition = offset;
            } else {
                // Otherwise, instantiate a new offset object and add it to the list
                GameObject child = Instantiate(empty);
                child.name = "Sys_Ally_Offset_" + i;
                child.transform.parent = gameObject.transform;
                child.transform.localPosition = offset;

                m_offsets.Add(child);
            }

            // Point each ally to their respective point
            m_allies[(int)m_startIdx % m_allies.Count].Target = m_offsets[i].transform;
        }
    }
}
