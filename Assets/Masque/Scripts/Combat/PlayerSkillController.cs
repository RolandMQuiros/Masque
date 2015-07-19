using UnityEngine;
using System.Collections;

public class PlayerSkillController : MonoBehaviour {
    private const int MAX_SKILLS = 4;
    private const string PRIMARY_FIRE_BTN = "Fire1";
    private const string DASH_BTN = "Dash";

    public PlayerSkill[] AttackSkills;
    public PlayerSkill DashSkill;
    
    private int m_activeAttackSkill = 0;

	public void Awake() {
	}
	
    /// <summary>
    /// Checks the Inputs for a Skill
    /// </summary>
	public void Update() {
        RunSkill(PRIMARY_FIRE_BTN, AttackSkills[m_activeAttackSkill]);
        RunSkill(DASH_BTN, DashSkill);
	}

    private void RunSkill(string button, PlayerSkill skill) {
        if (Input.GetButtonDown(button)) {
            skill.Press();

            for (int i = 0; i < AttackSkills.Length; i++) {
                if (AttackSkills[i] != skill && !AttackSkills[i].IsFinished) {
                    AttackSkills[i].Interrupt(skill);
                }
            }
            
            DashSkill.Interrupt(skill);
        } else if (Input.GetButton(button)) {
            skill.Hold();
        } else if (Input.GetButtonUp(button)) {
            skill.Release();
        }
    }
}
