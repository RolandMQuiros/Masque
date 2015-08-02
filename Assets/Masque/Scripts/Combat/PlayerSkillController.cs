using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSkillController : MonoBehaviour {
    private const int MAX_SKILLS = 4;
    private const string PRIMARY_FIRE_BTN = "Fire1";
    private const string DASH_BTN = "Dash";
    private const string CYCLE_FWD_BTN = "Cycle Forward";
    private const string CYCLE_BCK_BTN = "Cycle Backward";
    private const string DROP_BTN = "Drop";
    private const string PICKUP_BTN = "Pickup";

    public PlayerSkill[] AttackSkills;
    public PlayerSkill DashSkill;

    private PlayerAllyOrbiter m_orbiter;
    private PlayerAim m_aim;

    private uint m_activeAttackSkill = 0;

	public void Awake() {
        m_orbiter = GetComponent<PlayerAllyOrbiter>();
        m_aim = GetComponent<PlayerAim>();
	}
	
    /// <summary>
    /// Checks the Inputs for a Skill
    /// </summary>
	public void Update() {
        if (Input.GetButtonDown(CYCLE_FWD_BTN)) {
            m_activeAttackSkill = (uint)((m_activeAttackSkill + 1) % AttackSkills.Length);

            if (m_orbiter != null) {
                m_orbiter.CycleForward();
            }
        } else if (Input.GetButtonDown(CYCLE_BCK_BTN)) {
            m_activeAttackSkill = (uint)((m_activeAttackSkill - 1) % AttackSkills.Length);

            if (m_orbiter != null) {
                m_orbiter.CycleBackward();
            }
        }

        if (Input.GetButtonDown(DROP_BTN)) {
            AttackSkills[m_activeAttackSkill] = null;
            m_activeAttackSkill = (uint)((m_activeAttackSkill - 1) % AttackSkills.Length);

            DeferredFollower ally = m_orbiter.ActiveAlly;
            m_orbiter.RemoveAlly(ally);

            ally.Target = transform.position;
        }

        RunSkill(PRIMARY_FIRE_BTN, AttackSkills[m_activeAttackSkill]);
        RunSkill(DASH_BTN, DashSkill);
	}

    private void RunSkill(string button, PlayerSkill skill) {
        if (skill != null) {
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
}
