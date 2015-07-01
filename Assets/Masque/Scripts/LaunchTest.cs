using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Launchable))]
public class LaunchTest : MonoBehaviour {
    public float LaunchSpeed = 20f;
    public float Deceleration = 1000f;
    public int Bounces = 4;
    private Launchable m_launchable;
    private Vector3 m_startPos;

	// Use this for initialization
	public void Awake () {
        m_launchable = GetComponent<Launchable>();
        m_startPos = transform.position;
	}
	
	// Update is called once per frame
	public void Update () {
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(ray, out hitInfo);

            if (hit) {
                Vector3 velocity = LaunchSpeed * (hitInfo.point - transform.position).normalized;
                velocity.y = 0f;
                m_launchable.Launch(velocity, Deceleration, Bounces);
            }
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            transform.position = m_startPos;
            m_launchable.InterruptLaunch(true);
        }
	}
}
