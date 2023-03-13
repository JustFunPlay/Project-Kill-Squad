using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hitman Info", menuName = "ScriptableObjects/Character info/Hitman")]

public class HitmanData : CharacterInfoBase
{
    [Header("Ult")]
    public int ultDamage;
    public int requiredCrits;

}
