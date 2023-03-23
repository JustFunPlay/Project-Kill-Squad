using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Discipline of Trickery", menuName = "ScriptableObjects/Psyhic Discipline/Trickery")]
public class DisciplineOfTrickery : BasePsychicDiscipline
{
    public override bool PerformPsychicPower1(CharacterBase caster, CharacterBase target)
    {
        if ((power1Targeting != Targeting.Ally && caster.Owner != target.Owner) || (power1Targeting != Targeting.Enemy && caster.Owner == target.Owner))
        {
            Vector3 casterPos = caster.transform.position;
            Vector3 targetPos = target.transform.position;
            caster.transform.position = targetPos;
            target.transform.position = casterPos;
            return true;
        }
        return false;
    }
}
