using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerAim))]
public class PlayerCamera : MonoBehaviour {
    public CameraSpinner Camera;
    private PlayerAim m_aim;

	// Use this for initialization
	public void Awake() {
        m_aim = GetComponent<PlayerAim>();
	}
	
	// Update is called once per frame
	public void Update () {
	    Camera.Target = m_aim.Centroid;
	}
}
