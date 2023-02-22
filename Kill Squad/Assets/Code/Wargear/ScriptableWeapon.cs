using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scriptable Weapon", menuName = "ScriptableObjects/Weapons/Normal Weapon")]
public class ScriptableWeapon : ScriptableObject
{
    public string weaponName;
    public WeaponType type;
    public int attacks;
    public int range;
    public int armorPenetration;
    public int crit;
    public int damage;
    public int pointsCost;
}

public enum WeaponType
{
    Assault,
    RapidFire,
    Pistol,
    Spread,
    Combat,
    Melee
}
