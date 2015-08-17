using UnityEngine;
using System.Collections;

public class Ally : MonoBehaviour {
    public PlayerSkill AttackSkill;
    public PlayerSkill DashSkill;
    public DeferredFollower ParentFollower;
    public SphereCollider PickupCollider;

    public bool IsComplete {
        get {
            return AttackSkill != null &&
                   DashSkill != null &&
                   ParentFollower != null &&
                   PickupCollider != null;
        }
    }

    public void Awake() {
        if (PickupCollider == null) {
            PickupCollider = GetComponent<SphereCollider>();
        }
    }

    public void Start() {
        if (ParentFollower == null) {
            ParentFollower = GetComponentInParent<DeferredFollower>();
        }
    }

    public void Pickup() {
        PickupCollider.enabled = false;
    }

    public void Drop() {
        PickupCollider.enabled = true;
    }

	// Update is called once per frame
	public void Update () {
	
	}
}
