using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;


/// <summary>
/// Enumeration of different teams a Combatant can be on.
/// </summary>
public enum Faction {
    /// <summary>No team affiliation.  Attacks do not affect anyone.</summary>
    None,
    /// <summary>Player affiliation.  Offensive attacks affect Enemies and defensive/restorative affect friendlies.</summary>
    Player,
    /// <summary>Enemy affiliation.  Offensive attacks affect Players and defensive/restorative affect other Enemies</summary>
    Enemy,
    /// <summary>Attacks affect all other Team affiliations</summary>
    Hazard
}

/// <summary>
/// General description of a Combatant's stats.  Each unique Combatant type may interpret the information here differently. 
/// </summary>
public struct CombatantStats {
    public int MaxHealth;
    public int Strength;
    public int Defense;
    public int Agility;
    public int Mass;

    public Faction Faction;

    public static CombatantStats operator + (CombatantStats a, CombatantStats b) {
        return new CombatantStats {
            MaxHealth = a.MaxHealth + b.MaxHealth,
            Strength = a.Strength + b.Strength,
            Defense = a.Defense + b.Defense,
            Agility = a.Agility + b.Agility,
            Mass = a.Mass + b.Mass
        };
    }
}

public class CombatantModifier : IComparable<CombatantModifier> {
    public enum OperatorType {
        Add,
        Multiply
    }

    /// <summary>Whether or not the modifier acts on Base stats or Effective stats</summary>
    public enum EffectType {
        Base,
        Effective
    }

    public int ID;
    /// <summary>Only one modifier with the given ID can exist on a combatant</summary>
    public bool canStack;

    /// <summary>Indicates what kind of modifier operation this Modifier has on CombatantStats</summary>
    public OperatorType Operator;
    public EffectType Effect = EffectType.Base;
    public int Priority;
    
    public float HealthFactor;
    public float StrengthFactor;
    public float DefenseFactor;
    public float AgilityFactor;
    public float MassFactor;

    public CombatantModifier(OperatorType opType) {
        Operator = opType;
        
        switch (Operator) {
            case OperatorType.Add:
                HealthFactor = StrengthFactor = DefenseFactor = AgilityFactor = 0f;
                break;
            case OperatorType.Multiply:
                HealthFactor = StrengthFactor = DefenseFactor = AgilityFactor = 1f;
                break;
        }
    }

    public CombatantStats Apply(CombatantStats target) {
        switch (Operator) {
            case OperatorType.Add:
                target.MaxHealth   += (int)(HealthFactor   + 0.5f);
                target.Strength += (int)(StrengthFactor + 0.5f);
                target.Defense  += (int)(DefenseFactor  + 0.5f);
                target.Agility  += (int)(AgilityFactor  + 0.5f);
                target.Mass += (int)(MassFactor + 0.5f);
                break;
            case OperatorType.Multiply:
                target.MaxHealth   = (int)(target.MaxHealth * HealthFactor);
                target.Strength = (int)(target.Strength * StrengthFactor);
                target.Defense  = (int)(target.Defense * DefenseFactor);
                target.Agility  = (int)(target.Agility * AgilityFactor);
                target.Mass = (int)(target.Mass * MassFactor);
                break;
        }

        return target;
    }

    public int CompareTo(CombatantModifier other) {
        if (other.Priority > Priority) {
            return 1;
        } else if (other.Priority < Priority) {
            return -1;
        }

        return 0;
    }
}

/// <summary>
/// Provides a means to interact with CombatantStats
/// </summary>
[RequireComponent(typeof(CombatantStats))]
public class Combatant : MonoBehaviour {
    /// <summary>Base stats, or the stats of this Combatant when not affected by status effects or equipment modifiers</summary>
    private CombatantStats m_baseStats;
    /// <summary>Effective stats, or the stats of this Combatant when accounting for status effects or other modifiers.</summary>
    private CombatantStats m_effectiveStats;
    /// <summary>Flag which determines whether or not to recalculate effective stats</summary>
    private bool m_wereStatsUpdated = false;
    /// <summary>List of stat modifiers</summary>
    private List<CombatantModifier> m_modifiers = new List<CombatantModifier>();

    /// <summary>
    /// A Combatant's Base Stats, which represent its state free of status effects, equipment modifiers, etc.
    /// </summary>
    public CombatantStats BaseStats {
        get { return m_baseStats; }
        set {
            m_baseStats = value;
            m_wereStatsUpdated = true;
        }
    }
    
    /// <summary>
    /// A Combatant's Effective Stats, which represent its state after accounting for status effects, equipment
    /// modifiers, etc. This field is recalculated during any change in the Combatant's data, including when a modifier
    /// is added or removed, health is changed, etc.
    /// </summary>
    public CombatantStats EffectiveStats {
        get {
            if (m_wereStatsUpdated) {
                CalculateEffectiveStats();
                m_wereStatsUpdated = false;
            }
            return m_effectiveStats;
        }
    }

    /// <summary>
    /// The Combatant's current health, capped by EffectiveStats.MaxHealth.
    /// </summary>
    public int Health;

    public void AddModifier(CombatantModifier modifier) {
        m_modifiers.Add(modifier);
        m_wereStatsUpdated = true;
    }

    public void RemoveModifier(CombatantModifier modifier) {
        m_modifiers.Remove(modifier);
        m_wereStatsUpdated = true;
    }

    private void CalculateEffectiveStats() {
        CombatantStats baseAccumulator = m_baseStats;
        CombatantStats effectiveAccumulator = m_baseStats;

        m_modifiers.Sort();
        foreach (CombatantModifier modifier in m_modifiers) {
            switch (modifier.Effect) {
                case CombatantModifier.EffectType.Base:
                    CombatantStats modifiedStats = modifier.Apply(m_baseStats);
                    baseAccumulator += modifiedStats;
                    break;
                case CombatantModifier.EffectType.Effective:
                    effectiveAccumulator = modifier.Apply(effectiveAccumulator);
                    break;
            }
        }

        m_effectiveStats = effectiveAccumulator + baseAccumulator;
    }
}