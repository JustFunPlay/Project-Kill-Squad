using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Commando Info", menuName = "ScriptableObjects/Character info/Commando")]

public class CommandoData : CharacterInfoBase
{
    [Header("Equipment")]
    public ScriptableGrenade[] grenades;
    [Header("Ult")]
    public int minUltHits;
    public int maxUltHits;
    public int ultAp;
    public int ultDamage;
    public int requiredDamageDealt;
}
