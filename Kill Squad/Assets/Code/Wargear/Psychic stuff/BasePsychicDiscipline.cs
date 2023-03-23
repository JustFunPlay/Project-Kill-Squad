using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Psychic Discipline", menuName = "ScriptableObjects/Psyhic Discipline")]
public class BasePsychicDiscipline : ScriptableObject
{
    public string disciplineName;
    public int disciplineCost;

    [Header("Power 1")]
    public string power1Name;
    public int power1Cost;
    public int power1Strength;
    public int power1Duration;
    public bool power1Additive;
    public StatChange power1Buff;
    public Targeting power1Targeting;

    [Header("Power 2")]
    public string power2Name;
    public int power2Cost;
    public int power2Strength;
    public int power2Duration;
    public bool power2Additive;
    public StatChange power2Buff;
    public Targeting power2Targeting;

    [Header("Power 3")]
    public string power3Name;
    public int power3Cost;
    public int power3Strength;
    public int power3Duration;
    public bool power3Additive;
    public StatChange power3Buff;
    public Targeting power3Targeting;

    public virtual bool PerformPsychicPower1(CharacterBase caster, CharacterBase target)
    {
        if ((power1Targeting != Targeting.Ally && caster.Owner != target.Owner) || (power1Targeting != Targeting.Enemy && caster.Owner == target.Owner))
        {
            target.RecieveBuff(power1Buff, power1Strength, power1Duration, power1Additive);
            return true;
        }
        return false;
    }
    public virtual bool PerformPsychicPower2(CharacterBase caster, CharacterBase target)
    {
        if ((power2Targeting != Targeting.Ally && caster.Owner != target.Owner) || (power2Targeting != Targeting.Enemy && caster.Owner == target.Owner))
        {
            target.RecieveBuff(power2Buff, power2Strength, power2Duration, power2Additive);
            return true;
        }
        return false;
    }
    public virtual bool PerformPsychicPower3(CharacterBase caster, CharacterBase target)
    {
        if ((power3Targeting != Targeting.Ally && caster.Owner != target.Owner) || (power3Targeting != Targeting.Enemy && caster.Owner == target.Owner))
        {
            target.RecieveBuff(power3Buff, power3Strength, power3Duration, power3Additive);
            return true;
        }
        return false;
    }
}

public enum Targeting
{
    Ally,
    Enemy,
    Both
}