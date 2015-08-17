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
    private static int ms_interactLayer;

    public Ally DefaultAlly;

    private class SkillSet {
        public Ally Ally;
        public PlayerSkill AttackSkill { get; private set; }
        public PlayerSkill DashSkill { get; private set; }

        public SkillSet(Ally ally, PlayerSkill attack, PlayerSkill dash) {
            Ally = ally;
            AttackSkill = attack;
            DashSkill = dash;
        }
    }

    public Ally ActiveAlly {
        get {
            return m_skills.Get(0).Ally;
        }
    }

    private PlayerAllyOrbiter m_orbiter;
    private PlayerAim m_aim;

    private Deque<SkillSet> m_skills = new Deque<SkillSet>();
    private List<Ally> m_collidedAllies = new List<Ally>();

    private uint m_activeAttackSkill = 0;

	public void Awake() {
        m_orbiter = GetComponent<PlayerAllyOrbiter>();
        m_aim = GetComponent<PlayerAim>();

        ms_interactLayer = LayerMask.NameToLayer("Interactable");
	}

    public void Start() {
        if (DefaultAlly != null) {
            AddAlly(DefaultAlly);
        }
    }

    /// <summary>
    /// Checks the Inputs for a Skill
    /// </summary>
	public void Update() {
        if (m_skills.Count > 0) {
            if (Input.GetButtonDown(CYCLE_FWD_BTN)) {
                m_skills.AddBack(m_skills.RemoveFront());

                if (m_orbiter != null) {
                    m_orbiter.CycleForward();
                }
            } else if (Input.GetButtonDown(CYCLE_BCK_BTN)) {
                m_skills.AddFront(m_skills.RemoveBack());

                if (m_orbiter != null) {
                    m_orbiter.CycleBackward();
                }
            }

            if (Input.GetButtonDown(DROP_BTN)) {
                if (m_skills.Get(0).Ally != DefaultAlly) {
                    DeferredFollower ally = m_orbiter.ActiveAlly;
                    Ally pickup = ally.gameObject.GetComponentInChildren<Ally>();

                    // Enables the pickup collider
                    pickup.Drop();

                    // Find the closest available point between the Player and the Crosshair to place the
                    // dropped Ally
                    NavMeshHit hit;
                    if (NavMesh.Raycast(transform.position, m_aim.Target, out hit, NavMesh.AllAreas)) {
                        ally.Target = hit.position;
                    } else {
                        ally.Target = m_aim.Target;
                    }

                    // Remove ally
                    m_orbiter.RemoveFront();

                    // Remove ally's skillset
                    m_skills.RemoveFront();
                }
            }

            if (Input.GetButtonDown(PICKUP_BTN) && m_collidedAllies.Count > 0) {
                m_collidedAllies.Sort(DistanceSort);
                AddAlly(m_collidedAllies[0]);
            }
        }
        RunSkill(PRIMARY_FIRE_BTN, m_skills.Get(0).AttackSkill);
        RunSkill(DASH_BTN, m_skills.Get(0).DashSkill);
	}

    private bool AddAlly(Ally ally) {
        bool success = false;
        if (ally.IsComplete) {
            m_collidedAllies.Remove(ally);

            // Disable the pickup collider
            ally.Pickup();

            // Add follower to Ally Orbiter
            m_orbiter.AddAlly(ally.ParentFollower);

            // Copy Skill components into the Player object
            PlayerSkill attack = ally.AttackSkill.Clone(gameObject);
            PlayerSkill dash = ally.DashSkill.Clone(gameObject);

            // Add skills to Player object
            m_skills.AddBack(new SkillSet(ally, attack, dash));

            success = true;
        }

        return success;
    }

    public void OnTriggerEnter(Collider other) {
        // Check against interactable objects
        if (other.gameObject.layer == ms_interactLayer) {
            // Check for Ally Pickups
            Ally allySkill = other.gameObject.GetComponent<Ally>();
            // Add collided ally to list of nearby allies 
            if (allySkill != null) {
                if (!m_collidedAllies.Contains(allySkill)) {
                    m_collidedAllies.Add(allySkill);
                }
            }
        }
    }

    public void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == ms_interactLayer) {
            // Check for Ally Pickups
            Ally allySkill = other.gameObject.GetComponent<Ally>();

            // Remove collided ally from list of nearby allies
            if (allySkill != null) {
                if (m_collidedAllies.Contains(allySkill)) {
                    m_collidedAllies.Remove(allySkill);
                }
            }
        }
    }

    private void RunSkill(string button, PlayerSkill skill) {
        if (skill != null) {
            if (Input.GetButtonDown(button)) {
                skill.Press();

                for (int i = 0; i < m_skills.Count; i++) {
                    PlayerSkill attack = m_skills[i].AttackSkill;
                    if (attack != skill && !attack.IsFinished) {
                        attack.Interrupt(skill);
                    }

                    PlayerSkill dash = m_skills[i].DashSkill;
                    if (dash != skill && !dash.IsFinished) {
                        dash.Interrupt(skill);
                    }
                }
            } else if (Input.GetButton(button)) {
                skill.Hold();
            } else if (Input.GetButtonUp(button)) {
                skill.Release();
            }
        }
    }

    private int DistanceSort(Ally a, Ally b) {
        return Vector3.Distance(transform.position, a.transform.position).CompareTo(
                   Vector3.Distance(transform.position, b.transform.position)
               );
    }
}
