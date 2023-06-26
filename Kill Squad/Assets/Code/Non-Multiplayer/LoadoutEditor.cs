using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutEditor : MonoBehaviour
{
    [SerializeField] private KillSquad killSquad;
    [SerializeField] private int currentSelectedUnitIndex;

    [SerializeField] private int currentPointTotal;
    private PersistantInfo persistantInfo;
    [SerializeField] private CharacterLoadout[] defaultCharacters;

    private void Start()
    {
        persistantInfo = PersistantInfo.Instance;
        UpdatePointsValue();
    }
    private void UpdatePointsValue()
    {
        currentPointTotal = 0;
        for (int i = 0; i < persistantInfo.characters.Count; i++)
        {
            CharacterInfoBase character = killSquad.squad[persistantInfo.characters[i].Character];
            currentPointTotal += character.pointsCost;
            for (int ii = 0; ii < (character.has3slots ? 3 : 2); ii++)
            {
                currentPointTotal += character.weaponOptions[persistantInfo.characters[i].SelectedLoadoutOptions[ii]].pointsCost;

            }
            switch (persistantInfo.characters[i].Character)
            {
                case 2:
                    CommandoData comInfo = (CommandoData)character;
                    if (persistantInfo.characters[i].SelectedLoadoutOptions[4] == 1)
                        currentPointTotal += comInfo.grenades[persistantInfo.characters[i].SelectedLoadoutOptions[3]].extraCost;
                    else
                        currentPointTotal += comInfo.grenades[persistantInfo.characters[i].SelectedLoadoutOptions[3]].pointsCost;
                    break;
                case 5:
                    if (persistantInfo.characters[i].SelectedLoadoutOptions[2] == 1)
                        currentPointTotal += 20;
                        break;
            }
        }
    }

    public void AddCharacter(int character = 0)
    {
        persistantInfo.characters.Add(defaultCharacters[character]);
        UpdatePointsValue();
    }
    public void RemoveCharacter(int index)
    {
        persistantInfo.characters.RemoveAt(index);
        UpdatePointsValue();
    }
    public void ChangeCharacter(int index, int character)
    {
        persistantInfo.characters[index] = defaultCharacters[character];
        UpdatePointsValue();
    }
    public void ChangeEquipmentOption(int slot, int index)
    {
        persistantInfo.characters[currentSelectedUnitIndex].SelectedLoadoutOptions[slot] = index;
        UpdatePointsValue();
    }

}
