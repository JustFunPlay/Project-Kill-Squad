using System.Collections.Generic;
using UnityEngine;

public class ButtonSelectionLogic : MonoBehaviour
{
    [SerializeField] private Action action;
    [SerializeField] private ActionVar variation;
    public void ButtonTrigered()
    {
        GetComponentInParent<CharacterBase>().SelectAction(action, variation);
    }
}
