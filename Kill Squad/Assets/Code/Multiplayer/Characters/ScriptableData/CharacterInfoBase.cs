using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Info", menuName = "ScriptableObjects/Character info")]
public class CharacterInfoBase : ScriptableObject
{
    public CharacterBase physicalCharacter;
    public int speed;
    public int movement;
    public int health;
    public int armor;
    public int ranged;
    public int melee;
    public int attacks;

    public List<ScriptableWeapon> equipedWeapons = new List<ScriptableWeapon>();
}
