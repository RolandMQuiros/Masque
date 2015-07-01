using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {
    public float Distance;
    public float Speed;

    private float m_displacement;

    public void OnEnable() {
        m_displacement = 0f;
    }
    
	// Update is called once per frame
    public void Update () {
        if (m_displacement < Distance) {
            float offset = Speed * Time.deltaTime;
            m_displacement += offset;
            transform.position += transform.forward * offset;
        } else {
            gameObject.Recycle();
        }
    }
}
