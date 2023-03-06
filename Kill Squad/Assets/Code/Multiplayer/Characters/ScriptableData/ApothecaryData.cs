using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Apothecary Info", menuName = "ScriptableObjects/Character info/Apothecary")]

public class ApothecaryData : CharacterInfoBase
{
    public Vector2 healValue;
    public int healCharges;
}
