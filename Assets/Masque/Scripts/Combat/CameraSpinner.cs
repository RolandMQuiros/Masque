using UnityEngine;
using System.Collections;

/// <summary>
/// Constrains the camera object to a circular track around an axis
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraSpinner : MonoBehaviour {
    [Tooltip("Target transform")]
    public Transform Target;
    [Tooltip("Radius of the circular track surrounding the target")]
    public float TrackRadius = 10f;
    [Tooltip("How high or low the camera is offset from the XZ plane of the target")]
    public float TrackOffset = 5f;
    [Tooltip("How quickly, in degrees, the camera rotates about the target")]
    public float SpinSpeed = 5f;
    [Tooltip("Enable this to force the camera to face the target")]
    public bool LookAtTarget = true;

    public float Angle = 45f;

    private Camera m_camera;
    private Vector3 m_destinationPosition;

    public void Awake () {
        m_camera = GetComponent<Camera>();
	}
	
	public void LateUpdate () {
        if (Angle > 360f) {
            Angle -= 360f;
        } else if (Angle < 0f) {
            Angle += 360f;
        }

        float angleInRadians = Mathf.Deg2Rad * Angle;
        m_destinationPosition = new Vector3(
            Target.position.x + TrackRadius * Mathf.Cos(angleInRadians),
            Target.position.y + TrackOffset,
            Target.position.z + TrackRadius * Mathf.Sin(angleInRadians)
        );

        m_camera.transform.position += (m_destinationPosition - m_camera.transform.position) / 5f;

        m_camera.transform.rotation = Quaternion.LookRotation(Target.position - m_camera.transform.position);
	}
}
