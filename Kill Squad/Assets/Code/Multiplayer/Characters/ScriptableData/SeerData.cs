using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Seer Info", menuName = "ScriptableObjects/Character info/Seer")]

public class SeerData : CharacterInfoBase
{
    [Header("Psychic Powers")]
    public int psychicPoints;
    public Vector2 psychicGeneration;
    public int psychicRange;
    public BasePsychicDiscipline discipline1;
    public bool hasRunicArmor;

    [Header("Ult")]
    public int ultRange;
    public int requiredUltPoints;
}
