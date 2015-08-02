using UnityEngine;
using System.Collections;


/// <summary>
/// Provides functions that allow an object to move relative to a Camera
/// </summary>
public class PlanarViewVectors {
    public Transform ViewTransform;
    public float DirectionChangeThreshold = 0.25f;
    public float RefreshCameraVectorsThreshold = 0.1f;

    public Vector3 Forward { get; private set; }
    public Vector3 Right { get; private set; }
    public Vector3 Up { get; set; }
    
    private Plane m_plane;
    public Plane Plane {
        get { return m_plane; }
    }

    public PlanarViewVectors(Vector3 forward, Vector3 right, Vector3? up = null) {
        Forward = forward;
        Right = right;
        Up = up ?? Vector3.Cross(Forward, Right).normalized;
    }

    public PlanarViewVectors(Transform viewTransform) {
        RefreshBaseVectors(viewTransform);
    }

    public void RefreshBaseVectors(Transform viewTransform, Vector3? up = null) {
        if (viewTransform == null) {
            Forward = Vector3.forward;
            Right = Vector3.right;
        } else {
            Forward = (new Vector3(viewTransform.forward.x, 0f, viewTransform.forward.z)).normalized;
            Right = (new Vector3(viewTransform.right.x, 0f, viewTransform.right.z)).normalized;
        }

        if (up == null) {
            Up = Vector3.Cross(Forward, Right).normalized;
        } else {
            Up = up.Value;
        }
        m_plane.normal = Up;
    }

    public Vector3 Transform(Vector2 vec) {
        return (Forward * vec.y) + (Right * vec.x);
    }
}
