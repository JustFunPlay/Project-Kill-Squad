using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Infiltrator Info", menuName = "ScriptableObjects/Character info/Infiltrator")]

public class InfiltratorData : CharacterInfoBase
{
    [Header("Equipment")]
    public ScriptableWeapon primary;
    public ScriptableWeapon meleeWeapon;
}
