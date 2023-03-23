using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Discipline of Wrath", menuName = "ScriptableObjects/Psyhic Discipline/Wrath")]
public class DisciplineOfWrath : BasePsychicDiscipline
{
    public override bool PerformPsychicPower1(CharacterBase caster, CharacterBase target)
    {
        if ((power1Targeting != Targeting.Ally && caster.Owner != target.Owner) || (power1Targeting != Targeting.Enemy && caster.Owner == target.Owner))
        {
            target.TakeDamage(Random.Range(power1Strength, power1Duration + 1), true, out int DamageDealt, out bool killingBlow);
            if (killingBlow)
                caster.hasKilled = true;
            return true;
        }
        return false;
    }

    public override bool PerformPsychicPower2(CharacterBase caster, CharacterBase target)
    {
        if ((power2Targeting != Targeting.Ally && caster.Owner != target.Owner) || (power2Targeting != Targeting.Enemy && caster.Owner == target.Owner))
        {
            target.TakeDamage(Random.Range(power2Strength, power2Duration + 1), true, out int DamageDealt, out bool killingBlow);
            if (killingBlow)
                caster.hasKilled = true;
            caster.GetHealed(DamageDealt, out int healingDone);
            return true;
        }
        return false;
    }

    public override bool PerformPsychicPower3(CharacterBase caster, CharacterBase target)
    {
        if ((power2Targeting != Targeting.Ally && caster.Owner != target.Owner) || (power2Targeting != Targeting.Enemy && caster.Owner == target.Owner))
        {
            target.doubleNextHit = true;
            return true;
        }
        return false;
    }
}
