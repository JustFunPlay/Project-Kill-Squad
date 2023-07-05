using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeEquipment : MonoBehaviour
{
    [SerializeField] private int index;
    [SerializeField] private int value;
    [SerializeField] private LoadoutEditor editor;

    public void ChangeSelectedEquipment()
    {
        editor.ChangeEquipmentOption(index, value);
    }
}
