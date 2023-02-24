using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hitman Info", menuName = "ScriptableObjects/Character info/Hitman")]

public class HitmanData : CharacterInfoBase
{
    [Header("Equipment")]
    public ScriptableWeapon primary;
    public ScriptableWeapon sideArm;
    public ScriptableWeapon meleeWeapon;
}
