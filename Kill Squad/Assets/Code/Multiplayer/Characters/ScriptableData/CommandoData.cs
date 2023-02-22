using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Commando Info", menuName = "ScriptableObjects/Character info/Commando")]

public class CommandoData : CharacterInfoBase
{
    [Header("Equipment")]
    public ScriptableWeapon primary;
    public ScriptableWeapon sideArm;
    public ScriptableWeapon meleeWeapon;
    //public ScriptableGrenade grenade;
    public bool extraGrenades;
}
