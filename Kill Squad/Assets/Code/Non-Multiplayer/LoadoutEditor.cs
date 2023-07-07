using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutEditor : MonoBehaviour
{
    [SerializeField] private KillSquad killSquad;
    [SerializeField] private int currentSelectedUnitIndex;

    [SerializeField] private int currentPointTotal;
    //private PersistantInfo persistantInfo;
    [SerializeField] private CharacterLoadout[] defaultCharacters;
    [SerializeField] private List<CharacterLoadout> currentCharacters = new List<CharacterLoadout>();

    [Header("UI")] 
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMPro.TextMeshProUGUI pointsCount;

    [Header("ShowSquad")]
    [SerializeField] private GameObject[] exampleButtons;
    [SerializeField] private List<GameObject> currentList = new List<GameObject>();
    [SerializeField] private GameObject scrollFill;

    private void Start()
    {
        for (int i = 0; i < PersistantInfo.Instance.characters.Count; i++)
        {
            currentCharacters.Add(PersistantInfo.Instance.characters[i]);
            currentList.Add(Instantiate(exampleButtons[currentCharacters[i].Character], scrollFill.transform));
        }
        UpdatePointsValue();
    }
    private void UpdatePointsValue()
    {
        currentPointTotal = 0;
        for (int i = 0; i < currentCharacters.Count; i++)
        {
            CharacterInfoBase character = killSquad.squad[currentCharacters[i].Character];
            currentPointTotal += character.pointsCost;
            for (int ii = 0; ii < (character.has3slots ? 3 : 2); ii++)
            {
                currentPointTotal += character.weaponOptions[currentCharacters[i].SelectedLoadoutOptions[ii]].pointsCost;

            }
            switch (currentCharacters[i].Character)
            {
                case 2:
                    CommandoData comInfo = (CommandoData)character;
                    if (currentCharacters[i].SelectedLoadoutOptions[4] == 1)
                        currentPointTotal += comInfo.grenades[currentCharacters[i].SelectedLoadoutOptions[3]].extraCost;
                    else
                        currentPointTotal += comInfo.grenades[currentCharacters[i].SelectedLoadoutOptions[3]].pointsCost;
                    break;
                case 5:
                    if (currentCharacters[i].SelectedLoadoutOptions[2] == 1)
                        currentPointTotal += 20;
                        break;
            }
        }
        pointsCount.text = $"{currentPointTotal}/500";
        if (currentPointTotal <= 500 && currentPointTotal >= 450)
        {
            pointsCount.color = Color.green;
            confirmButton.interactable = true;
        }
        else
        {
            pointsCount.color = Color.red;
            confirmButton.interactable = false;
        }
    }

    public void AddCharacter(int character = 0)
    {
        currentCharacters.Add(defaultCharacters[character]);
        currentList.Add(Instantiate(exampleButtons[character], scrollFill.transform));
        UpdatePointsValue();
    }
    public void RemoveCharacter(GameObject character)
    {
        for (int i = 0; i < currentList.Count; i++)
        {
            if (character != currentList[i])
                continue;
            currentCharacters.RemoveAt(i);
            Destroy(currentList[i]);
            currentList.RemoveAt(i);
            i--;
        }
        UpdatePointsValue();
    }
    public void ChangeCharacter(int index, int character)
    {
        currentCharacters[index] = defaultCharacters[character];
        UpdatePointsValue();
    }
    public void ChangeEquipmentOption(int slot, int index)
    {
        currentCharacters[currentSelectedUnitIndex].SelectedLoadoutOptions[slot] = index;
        UpdatePointsValue();
    }

    public void ConfirmSquad()
    {
        PersistantInfo.Instance.characters.Clear();
        for (int i = 0; i < currentCharacters.Count; i++)
        {
            PersistantInfo.Instance.characters.Add(currentCharacters[i]);
        }
    }

}
