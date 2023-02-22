using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Apothecary Info", menuName = "ScriptableObjects/Character info/Apothecary")]

public class ApothecaryData : CharacterInfoBase
{
    [Header("Equipment")]
    public ScriptableWeapon primary;
    public ScriptableWeapon sideArm;
    public ScriptableWeapon meleeWeapon;
    public Vector2 healValue;
    public int healCharges;
}
