using UnityEngine;
using System.Collections;

public abstract class PlayerSkill : MonoBehaviour {
    [Tooltip("How much the player can use this skill")]
    public int Ammunition = 100;
    
    [Tooltip("How long after usage the skill delays before recovering.")]
    public float RecoveryDelay = 1f;
    
    [Tooltip("How much Ammunition a skill recovers per second.")]
    public float RecoveryRate = 1f;

    [Tooltip("If the player uses a skill more than its Ammunition can support, it overloads, rendering it unusable " +
             "a certain amount of time.")]
    public bool CanOverload = true;
    
    [Tooltip("How long the player must wait until an overloaded skill can begin recovering.")]
    public float OverloadDelay = 5f;
    
    [Tooltip("Whether or not this Skill can be used.  Usually disabled when the player is hurt or stunned, or when " +
             "other skills are currently running.")]
    public bool IsUsable = true;

    public bool IsFinished = true;

    public virtual void Press() { }
    public virtual void Hold() { }
    public virtual void Release() { }
    public virtual void Interrupt() { }
    public virtual void Interrupt(PlayerSkill other) { }
}
