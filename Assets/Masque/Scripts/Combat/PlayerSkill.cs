using UnityEngine;
using System.Collections;

public abstract class PlayerSkill {
    public GameObject Owner;

    public PlayerSkill(GameObject owner) {
        Owner = owner;
    }

    public abstract void Start();
    public abstract void Hold();
    public abstract void Release();
    public abstract void Update();
}
