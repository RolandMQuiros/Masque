using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawn {
    public GameObject Prefab;
    public int Count;

    public Spawn(GameObject prefab = null, int count = 0) {
        Prefab = prefab;
        Count = count;
    }
}

public class SpawnField : MonoBehaviour {
    [Tooltip("Target position.  If assigned, this SpawnField will generate")]
    public Vector3 Target;

    public List<List<Spawn>> SpawnWaves = new List<List<Spawn>>();


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
