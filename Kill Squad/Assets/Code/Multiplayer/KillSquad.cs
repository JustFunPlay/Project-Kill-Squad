using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New KilSquad", menuName = "ScriptableObjects/KillSquad")]
public class KillSquad : ScriptableObject
{
    public List<CharacterInfoBase> squad = new List<CharacterInfoBase>();
}
