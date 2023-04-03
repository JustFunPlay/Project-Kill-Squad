using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageSquadCreator : MonoBehaviour
{

}

[System.Serializable]
public class CharacterLoadout
{
    public CharacterInfoBase Character;
    public int[] SelectedLoadoutOptions;
}