using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Seer Info", menuName = "ScriptableObjects/Character info/Seer")]

public class SeerData : CharacterInfoBase
{
    public int psychicPoints;
    public Vector2 psychicGeneration;
    public bool hasRunicArmor;
}
