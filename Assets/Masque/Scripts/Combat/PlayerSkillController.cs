using UnityEngine;
using System.Collections;

public class PlayerSkillController : MonoBehaviour {
    private const int MAX_SKILLS = 4;

    public PlayerSkill[] Skills;
    public string[] Buttons;

	public void Awake() {
	}
	
    /// <summary>
    /// Checks the Inputs for a Skill
    /// </summary>
	public void Update() {
        for (int i = 0; i < Skills.Length; i++) {
            if (Skills[i] != null && Buttons[i] != null) {
                PlayerSkill skill = Skills[i];
                string button = Buttons[i];

                if (Input.GetButtonDown(button)) {
                    skill.Press();

                    // Interrupt currently running skills
                    for (int j = 0; j < Skills.Length; j++) {
                        if (i != j && Skills[j] && !Skills[j].IsFinished) {
                            Skills[j].Interrupt(skill);
                        }
                    }

                } else if (Input.GetButton(button)) {
                    skill.Hold();
                } else if (Input.GetButtonUp(button)) {
                    skill.Release();
                }
            }
        }
	}
}
