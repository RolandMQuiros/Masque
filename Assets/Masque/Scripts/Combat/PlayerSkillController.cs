using UnityEngine;
using System.Collections;

public class PlayerSkillController : MonoBehaviour {
    private PlayerSkill Skill;
    public string Button;

    public Bullet QuickBoltPrefab;
    public Bullet StunBoltPrefab;

	public void Awake() {
        Skill = new BoltSkill(gameObject, QuickBoltPrefab, StunBoltPrefab);
	}
	
    /// <summary>
    /// Checks the Inputs for a Skill
    /// </summary>
	public void Update() {
        if (Input.GetButtonDown(Button)) {
            Skill.Start();
        } else if (Input.GetButton(Button)) {
            Skill.Hold();
        } else if (Input.GetButtonUp(Button)) {
            Skill.Release();
        }

        Skill.Update();
	}
}
