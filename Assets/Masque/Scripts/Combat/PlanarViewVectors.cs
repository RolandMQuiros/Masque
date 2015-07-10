using UnityEngine;
using System.Collections;


/// <summary>
/// Provides functions that allow an object to move relative to a Camera
/// </summary>
public class PlanarViewVectors : MonoBehaviour {
    public float DIRECTION_CHANGE_THRESHOLD = 0.25f;
    public float REFRESH_CAMERA_VECTORS_THRESHOLD = 0.1f;
    

    public Vector3 Forward { get; private set; }
    public Vector3 Right { get; private set; }
    public Vector3 Up { get; private set; }
    
    private Plane m_plane;
    public Plane Plane {
        get { return m_plane; }
    }
    

    public void RefreshBaseVectors(Transform viewTransform) {
        if (viewTransform == null) {
            Forward = Vector3.forward;
            Right = Vector3.right;
        } else {
            Forward = (new Vector3(viewTransform.forward.x, 0f, viewTransform.forward.z)).normalized;
            Right = (new Vector3(viewTransform.right.x, 0f, viewTransform.right.z)).normalized;
        }
        Up = Vector3.Cross(Right, Forward).normalized;

        m_plane = new Plane(Up, Vector3.Dot(transform.position, Up));
    }

    public Vector3 Apply(Vector2 vec) {
        return (Forward * vec.y) + (Right * vec.x);
    }

    public void Update() {
        m_plane.distance = Vector3.Dot(transform.position, Up);
    }
}
