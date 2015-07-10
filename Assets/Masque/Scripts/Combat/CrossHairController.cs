using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PlanarViewVectors))]
public class CrossHairController : MonoBehaviour {
    public float MovementSpeed = 50f;
    public Camera Camera;
    
    private NavMeshAgent m_agent;
    private PlanarViewVectors m_planeView;
	
    // Use this for initialization
	public void Awake () {
        m_agent = GetComponent<NavMeshAgent>();
        m_planeView = GetComponent<PlanarViewVectors>();
	}
	
	// Update is called once per frame
	public void Update () {
        Vector2 axes = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        m_planeView.RefreshBaseVectors(Camera.transform);
        Vector3 velocity = m_planeView.Apply(axes) * MovementSpeed * Time.deltaTime;
        m_agent.Move(velocity);
	}
}
