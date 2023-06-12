using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageSquadCreator : MonoBehaviour
{

}

[System.Serializable]
public class CharacterLoadout
{
    public int Character;
    public List<int> SelectedLoadoutOptions = new List<int>();
}

//[System.Serializable]
//public enum CharacterType
//{
//    Apothecary,
//    ArcTrooper,
//    Commando,
//    Hitman,
//    Infiltrator,
//    Seer
//}