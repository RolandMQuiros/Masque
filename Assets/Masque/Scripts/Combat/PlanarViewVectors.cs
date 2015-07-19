using UnityEngine;
using System.Collections;


/// <summary>
/// Provides functions that allow an object to move relative to a Camera
/// </summary>
public class PlanarViewVectors : MonoBehaviour {
    public Transform ViewTransform;
    public float DirectionChangeThreshold = 0.25f;
    public float RefreshCameraVectorsThreshold = 0.1f;

    public Vector3 Forward;// { get; private set; }
    public Vector3 Right;// { get; private set; }
    public Vector3 Up;// { get; private set; }
    
    private Plane m_plane;
    public Plane Plane {
        get { return m_plane; }
    }

    public float DebugDistance;

    public void RefreshBaseVectors(Transform viewTransform) {
        if (viewTransform == null) {
            Forward = Vector3.forward;
            Right = Vector3.right;
        } else {
            Forward = (new Vector3(viewTransform.forward.x, 0f, viewTransform.forward.z)).normalized;
            Right = (new Vector3(viewTransform.right.x, 0f, viewTransform.right.z)).normalized;
        }
        Up = Vector3.Cross(Forward, Right).normalized;

        m_plane = new Plane(Up, transform.position);//Vector3.Dot(transform.position, Up));
    }

    public Vector3 Apply(Vector2 vec) {
        return (Forward * vec.y) + (Right * vec.x);
    }

    public void Update() {
        DebugDistance = m_plane.distance = Vector3.Dot(transform.position, Up);
    }

    public void LateUpdate() {
        RefreshBaseVectors(ViewTransform);
    }
}
