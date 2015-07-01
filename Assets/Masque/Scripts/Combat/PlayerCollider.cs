using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerCollider : MonoBehaviour {
    public PlayerController PlayerController;

    public void Awake() {
    }
    public void OnTriggerEnter(Collider collider) {
        Debug.Log("Trigger Entered: " + collider.gameObject.name);
    }

    public void OnTriggerExit(Collider collider) {
        Debug.Log("Trigger Exited: " + collider.gameObject.name);
    }
}
