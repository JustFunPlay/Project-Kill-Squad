using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scriptable Grenade", menuName = "ScriptableObjects/Weapons/Grenade")]
public class ScriptableGrenade : ScriptableObject
{
    public string weaponName;
    public Vector2 attacks;
    public int range;
    public int armorPenetration;
    public int crit;
    public int damage;
    public int pointsCost;
    public int count;

    [Header("extra")]
    public int extraCost;
    public int increasedCount;
}
