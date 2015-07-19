using UnityEngine;
using System.Collections;

public class CursorDebug : MonoBehaviour {
    public PlayerAim Aim;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	    transform.position = Aim.Target;
	}
}
