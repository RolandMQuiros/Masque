using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Keeps track of platforms beneath a non-rigid body object.  Accounts for moving platforms.
/// Does not yet account for rotations or scales.
/// </summary>
public class PlatformDetector : MonoBehaviour {
    public Vector3 Start;
    public Vector3 End = new Vector3(0f, 1f, 0f);
    public LayerMask Layer;
    
    private bool m_wasPlatformFound;
    public bool WasPlatformFound {
        get { return m_wasPlatformFound; }
    }

    private Vector3 m_offset;
    public Vector3 Offset {
        get { return m_offset; }
    }

    public string DebugCast;
    public Vector3 DebugOffset;

    private GameObject m_currentPlatform;
    private Vector3 m_prevPosition;

    public void Update() {
        RaycastHit hitInfo;
        m_offset = Vector3.zero;

        m_wasPlatformFound = Physics.Linecast(transform.position + Start,
                                         transform.position + End,
                                         out hitInfo,
                                         Layer.value);

        if (m_wasPlatformFound) {
            // If this is the same platform from the previous frame, transform this object
            // to match it
            if (hitInfo.collider.gameObject == m_currentPlatform) {
                // Offset to contact ground
                m_offset = hitInfo.point - transform.position;
                    
                // Calculate translation offset
                m_offset += m_currentPlatform.transform.position - m_prevPosition;
                m_prevPosition = m_currentPlatform.transform.position;
            } else {
                m_currentPlatform = hitInfo.collider.gameObject;
                m_prevPosition = m_currentPlatform.transform.position;
            }

            DebugCast = hitInfo.collider.gameObject.name;
        } else {
            DebugCast = m_wasPlatformFound.ToString();
        }
    }
}
